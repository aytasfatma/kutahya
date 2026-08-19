using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.ProductImageImport;

/// <summary>
/// Ürün koduna göre toplu görsel eşleştirme aracı. Varsayılan ve ilk aşama daima dry-run'dır.
/// Apply işlemi yalnızca aynı kaynak/veritabanı durumundan üretilmiş, bloksuz bir JSON raporu
/// --approved-report ile açıkça verilirse çalışır.
/// </summary>
public sealed partial class ProductImageImportCliService
{
    private const string PlaceholderFileName = "deneme.webp";
    private const string ImportFolder = "products/imported";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { ".webp", ".jpg", ".jpeg", ".png" };

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductImageImportCliService> _logger;

    public ProductImageImportCliService(
        AppDbContext db,
        IWebHostEnvironment environment,
        ILogger<ProductImageImportCliService> logger)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
    }

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            Console.Error.WriteLine("Kullanım: product-image-import <kaynak-dizin> [--report-dir <dizin>] [--apply --approved-report <json>]");
            return 2;
        }

        var sourceRoot = Path.GetFullPath(args[0]);
        if (!Directory.Exists(sourceRoot))
        {
            Console.Error.WriteLine($"Kaynak dizin bulunamadı: {sourceRoot}");
            return 2;
        }

        var apply = args.Any(x => x.Equals("--apply", StringComparison.OrdinalIgnoreCase));
        var reportDirectory = Path.GetFullPath(Option(args, "--report-dir")
            ?? Path.Combine(_environment.ContentRootPath, "product-image-import-reports"));
        var approvedReport = Option(args, "--approved-report");

        try
        {
            var report = await AnalyzeAsync(sourceRoot);
            if (!apply)
            {
                Directory.CreateDirectory(reportDirectory);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                var jsonPath = Path.Combine(reportDirectory, $"product-images-dry-run-{stamp}.json");
                var htmlPath = Path.Combine(reportDirectory, $"product-images-dry-run-{stamp}.html");
                report.ReportToken = ComputeToken(report);
                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(report, JsonOptions), Encoding.UTF8);
                await File.WriteAllTextAsync(htmlPath, BuildHtml(report, jsonPath), Encoding.UTF8);
                PrintSummary(report, jsonPath, htmlPath);
                return report.HasBlockers ? 3 : 0;
            }

            if (string.IsNullOrWhiteSpace(approvedReport) || !File.Exists(approvedReport))
            {
                Console.Error.WriteLine("Apply için önce üretilen JSON raporu --approved-report ile vermelisiniz.");
                return 2;
            }

            var approved = JsonSerializer.Deserialize<ImportReport>(await File.ReadAllTextAsync(approvedReport), JsonOptions);
            var currentToken = ComputeToken(report);
            if (approved is null || approved.HasBlockers || string.IsNullOrWhiteSpace(approved.ReportToken)
                || !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(approved.ReportToken), Encoding.UTF8.GetBytes(currentToken)))
            {
                Console.Error.WriteLine("Onaylı rapor bloksuz değil veya kaynak/veritabanı durumu rapordan sonra değişmiş. Yeni dry-run gerekli.");
                return 4;
            }

            await ApplyAsync(report);
            Console.WriteLine($"Import tamamlandı. Ürün: {report.MatchedProductCount}, görsel: {report.PlannedImageCount}");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün görsel import işlemi başarısız oldu.");
            Console.Error.WriteLine($"İşlem başarısız: {ex.Message}");
            return 1;
        }
    }

    private async Task<ImportReport> AnalyzeAsync(string sourceRoot)
    {
        var fallbackSource = Path.GetFullPath(Path.Combine(sourceRoot, "..", "AgeçiciGörsel", PlaceholderFileName));
        if (!File.Exists(fallbackSource))
            throw new FileNotFoundException("Yeni görseli bulunmayan ürünler için fallback deneme.webp bulunamadı.", fallbackSource);

        var products = await _db.Products.AsNoTracking()
            .OrderBy(p => p.ProductCode)
            .Select(p => new ProductRecord(p.Id, p.ProductCode, p.CommercialName))
            .ToListAsync();
        var existingImages = await _db.ProductImages.AsNoTracking()
            .Select(i => new ExistingImage(i.ProductId, i.FilePath, i.IsPrimary, i.DisplayOrder)).ToListAsync();

        var exact = products.GroupBy(p => NormalizeCode(p.Code)).ToDictionary(g => g.Key, g => g.ToList());
        var relaxed = products.GroupBy(p => RelaxCode(p.Code)).ToDictionary(g => g.Key, g => g.ToList());
        var files = Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
        var sources = new List<SourceAnalysis>(files.Count);

        foreach (var path in files)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var candidate = ExtractCode(name);
            var matches = new List<ProductRecord>();
            var matchKind = "none";
            if (candidate is not null && exact.TryGetValue(NormalizeCode(candidate), out var exactMatches))
            {
                matches = exactMatches;
                matchKind = matches.Count == 1 ? "exact" : "ambiguous";
            }
            else if (candidate is not null && relaxed.TryGetValue(RelaxCode(candidate), out var relaxedMatches))
            {
                matches = relaxedMatches;
                matchKind = matches.Count == 1 ? "normalized-separator" : "ambiguous";
            }

            var info = await Image.IdentifyAsync(path);
            var split = DetectSplit(name, info.Width, info.Height);
            sources.Add(new SourceAnalysis
            {
                RelativePath = Path.GetRelativePath(sourceRoot, path).Replace('\\', '/'),
                FullPath = path,
                Length = new FileInfo(path).Length,
                LastWriteUtcTicks = File.GetLastWriteTimeUtc(path).Ticks,
                CandidateCode = candidate,
                MatchKind = matchKind,
                ProductId = matches.Count == 1 ? matches[0].Id : null,
                ProductCode = matches.Count == 1 ? matches[0].Code : null,
                ProductName = matches.Count == 1 ? matches[0].Name : null,
                CandidateProductCodes = matches.Select(m => m.Code).ToList(),
                Width = info.Width,
                Height = info.Height,
                PartCount = split.PartCount,
                SplitConfidence = split.Confidence,
                SplitReason = split.Reason,
                SplitXs = split.SplitXs
            });
        }

        var matchedGroups = sources.Where(s => s.ProductId.HasValue && s.SplitConfidence != "uncertain")
            .GroupBy(s => s.ProductId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var plans = new List<ProductPlan>();
        foreach (var product in products)
        {
            var productSources = matchedGroups.GetValueOrDefault(product.Id) ?? [];
            var current = existingImages.Where(i => i.ProductId == product.Id).OrderBy(i => i.DisplayOrder).ToList();
            var images = new List<PlannedImage>();
            foreach (var source in productSources.OrderBy(s => s.RelativePath, StringComparer.OrdinalIgnoreCase))
            {
                var sourceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    source.RelativePath.ToUpperInvariant())))[..12].ToLowerInvariant();
                for (var part = 1; part <= source.PartCount; part++)
                {
                    var safeCode = SafeName(product.Code);
                    var fileName = $"{safeCode}-kaynak-{sourceKey}-parca{part:00}.webp";
                    images.Add(new PlannedImage(source.RelativePath, source.FullPath, part, source.PartCount,
                        $"/uploads/{ImportFolder}/{safeCode}/{fileName}"));
                }
            }

            var usesFallback = images.Count == 0;
            if (usesFallback)
            {
                images.Add(new PlannedImage("[fallback]/deneme.webp", fallbackSource, 1, 1,
                    $"/uploads/{ImportFolder}/fallback/{PlaceholderFileName}"));
            }

            plans.Add(new ProductPlan
            {
                ProductId = product.Id,
                ProductCode = product.Code,
                ProductName = product.Name,
                SourceCount = productSources.Count,
                PlannedImages = images,
                PreservedImages = current.Where(i => !IsPlaceholder(i.FilePath)).Select(i => i.FilePath).ToList(),
                PlaceholderRecordsToRemove = current.Count(i => IsPlaceholder(i.FilePath)),
                CouldBecomeImageless = false,
                UsesFallback = usesFallback
            });
        }

        return new ImportReport
        {
            GeneratedAtUtc = DateTime.UtcNow,
            SourceRoot = sourceRoot,
            DatabaseProductCount = products.Count,
            SourceImageCount = sources.Count,
            Sources = sources,
            Products = plans,
            ExistingStateFingerprint = FingerprintExisting(products, existingImages)
        };
    }

    private async Task ApplyAsync(ImportReport report)
    {
        if (report.HasBlockers) throw new InvalidOperationException("Belirsizlik içeren rapor uygulanamaz.");
        var createdFiles = new List<string>();
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var plan in report.Products.Where(p => p.PlannedImages.Count > 0))
            {
                var existing = await _db.ProductImages.Where(i => i.ProductId == plan.ProductId)
                    .OrderBy(i => i.DisplayOrder).ToListAsync();
                var placeholder = existing.Where(i => IsPlaceholder(i.FilePath)).ToList();
                foreach (var image in placeholder) image.SetPrimary(false);
                _db.ProductImages.RemoveRange(placeholder);

                var nextOrder = existing.Where(i => !IsPlaceholder(i.FilePath)).Select(i => i.DisplayOrder)
                    .DefaultIfEmpty(-1).Max() + 1;
                var hasPrimary = existing.Any(i => !IsPlaceholder(i.FilePath) && i.IsPrimary);
                foreach (var planned in plan.PlannedImages)
                {
                    var relative = planned.TargetPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var target = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relative));
                    var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads"));
                    if (!target.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Güvensiz hedef yolu reddedildi.");

                    var alreadyInDb = existing.Any(i => i.FilePath.Equals(planned.TargetPath, StringComparison.OrdinalIgnoreCase));
                    if (alreadyInDb) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    if (!File.Exists(target))
                    {
                        if (planned.PartCount == 1
                            && Path.GetExtension(planned.SourceFullPath).Equals(".webp", StringComparison.OrdinalIgnoreCase))
                        {
                            // Tekli kaynak zaten WebP: yeniden kodlamak hem yavaş hem de gereksiz kalite kaybıdır.
                            File.Copy(planned.SourceFullPath, target, overwrite: false);
                        }
                        else
                        {
                            using var image = await Image.LoadAsync(planned.SourceFullPath);
                            var bounds = PartBounds(image.Width, image.Height, planned.PartNumber, planned.PartCount);
                            if (planned.PartCount > 1) image.Mutate(x => x.Crop(bounds));
                            await image.SaveAsync(target, new WebpEncoder { Quality = 92, FileFormat = WebpFileFormatType.Lossy });
                        }
                        createdFiles.Add(target);
                    }

                    var entity = new ProductImage(plan.ProductId, ProductImageType.Render, planned.TargetPath, !hasPrimary, nextOrder++);
                    hasPrimary = true;
                    await _db.ProductImages.AddAsync(entity);
                }
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            foreach (var path in createdFiles)
                try { File.Delete(path); } catch (Exception cleanup) { _logger.LogError(cleanup, "Yarım kalan dosya temizlenemedi: {Path}", path); }
            throw;
        }
    }

    private static SplitDetection DetectSplit(string name, int width, int height)
    {
        if (TripleMarker().IsMatch(name))
        {
            if (width >= 3 && width >= height)
                return new(3, "high", "Dosya adındaki 3'lü pano işareti ve yatay tuval oranı", [width / 3, width * 2 / 3]);
            return new(1, "uncertain", "Dosya adı 3'lü diyor ancak tuval geometrisi uyumsuz", []);
        }
        if (DoubleMarker().IsMatch(name))
        {
            if (width >= 2 && width >= height)
                return new(2, "high", "Dosya adındaki 2'li pano işareti ve yatay tuval oranı", [width / 2]);
            return new(1, "uncertain", "Dosya adı 2'li diyor ancak tuval geometrisi uyumsuz", []);
        }
        return new(1, "high", "Birleşik pano işareti yok; tek kaynak görsel", []);
    }

    private static Rectangle PartBounds(int width, int height, int part, int count)
    {
        var left = (part - 1) * width / count;
        var right = part * width / count;
        return new Rectangle(left, 0, right - left, height);
    }

    private static string? ExtractCode(string fileName)
    {
        var prefix = fileName.Split(" - ", 2, StringSplitOptions.TrimEntries)[0].Trim();
        var match = LeadingCode().Match(prefix);
        return match.Success ? match.Value : null;
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string RelaxCode(string value) => Separator().Replace(NormalizeCode(value), string.Empty);
    private static string SafeName(string value) => UnsafeName().Replace(value.Trim().ToLowerInvariant(), "-").Trim('-');
    private static bool IsPlaceholder(string path) => Path.GetFileName(path).Equals(PlaceholderFileName, StringComparison.OrdinalIgnoreCase);

    private static string FingerprintExisting(IEnumerable<ProductRecord> products, IEnumerable<ExistingImage> images)
    {
        var text = string.Join('\n', products.Select(p => $"P|{p.Id}|{p.Code}")) + "\n" +
            string.Join('\n', images.OrderBy(i => i.ProductId).ThenBy(i => i.DisplayOrder)
                .Select(i => $"I|{i.ProductId}|{i.FilePath}|{i.IsPrimary}|{i.DisplayOrder}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }

    private static string ComputeToken(ImportReport report)
    {
        var source = string.Join('\n', report.Sources.Select(s => $"{s.RelativePath}|{s.Length}|{s.LastWriteUtcTicks}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{Path.GetFullPath(report.SourceRoot)}\n{report.ExistingStateFingerprint}\n{source}")));
    }

    private static string? Option(string[] args, string name)
    {
        var index = Array.FindIndex(args, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static void PrintSummary(ImportReport r, string json, string html)
    {
        Console.WriteLine("DRY-RUN tamamlandı; veritabanı ve ürün görselleri değiştirilmedi.");
        Console.WriteLine($"Ürün: {r.DatabaseProductCount}, kaynak: {r.SourceImageCount}, eşleşen ürün: {r.MatchedProductCount}");
        Console.WriteLine($"Tekli: {r.SingleCount}, ikili: {r.DoubleCount}, üçlü: {r.TripleCount}, belirsiz bölme: {r.UncertainSplitCount}");
        Console.WriteLine($"Eşleşmeyen: {r.UnmatchedCount}, belirsiz eşleşme: {r.AmbiguousMatchCount}, planlanan görsel: {r.PlannedImageCount}");
        Console.WriteLine($"JSON: {json}");
        Console.WriteLine($"HTML: {html}");
        if (r.HasBlockers) Console.WriteLine("BLOKE: Belirsizlikler çözülmeden apply yapılamaz.");
    }

    private static string BuildHtml(ImportReport r, string jsonPath)
    {
        static string H(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        var b = new StringBuilder("<!doctype html><html lang=\"tr\"><head><meta charset=\"utf-8\"><title>Ürün Görsel Dry-run</title><style>body{font:14px system-ui;margin:24px;color:#222}table{border-collapse:collapse;width:100%}th,td{padding:7px;border:1px solid #ddd;text-align:left}.ok{color:#087830}.bad{color:#b42318}.cards{display:grid;grid-template-columns:repeat(auto-fill,minmax(360px,1fr));gap:18px}.card{border:1px solid #ccc;padding:12px}.visual{position:relative;display:inline-block;max-width:100%}.visual img{display:block;max-width:100%;max-height:360px}.cut{position:absolute;top:0;bottom:0;width:3px;background:#e11d48}.parts{display:flex;gap:8px;margin-top:8px}.part{background-size:auto 100%;background-repeat:no-repeat;height:180px;flex:1;border:1px solid #aaa}</style></head><body>");
        b.Append($"<h1>Ürün görsel import dry-run</h1><p>Üretim: {r.GeneratedAtUtc:u}<br>JSON: {H(jsonPath)}<br><strong class='{(r.HasBlockers ? "bad" : "ok")}'>{(r.HasBlockers ? "Apply bloke" : "Apply için uygun")}</strong></p>");
        b.Append($"<table><tr><th>DB ürün</th><th>Kaynak</th><th>Eşleşen ürün</th><th>Eşleşmeyen</th><th>Belirsiz eşleşme</th><th>Tek/2/3</th><th>Belirsiz bölme</th><th>Planlanan</th><th>Placeholder kaldırma</th></tr><tr><td>{r.DatabaseProductCount}</td><td>{r.SourceImageCount}</td><td>{r.MatchedProductCount}</td><td>{r.UnmatchedCount}</td><td>{r.AmbiguousMatchCount}</td><td>{r.SingleCount}/{r.DoubleCount}/{r.TripleCount}</td><td>{r.UncertainSplitCount}</td><td>{r.PlannedImageCount}</td><td>{r.PlaceholderRemovalCount}</td></tr></table>");
        b.Append("<h2>Bölünecek / belirsiz kaynaklar</h2><div class='cards'>");
        foreach (var s in r.Sources.Where(x => x.PartCount > 1 || x.SplitConfidence == "uncertain"))
        {
            var uri = new Uri(s.FullPath).AbsoluteUri;
            b.Append($"<article class='card'><h3>{H(s.ProductCode ?? s.CandidateCode ?? "Kod yok")}</h3><p>{H(s.RelativePath)}<br>{H(s.ProductName)}<br>{s.Width}×{s.Height}; {s.PartCount} parça; {H(s.SplitReason)}</p><div class='visual'><img src='{H(uri)}'>");
            foreach (var x in s.SplitXs) b.Append($"<span class='cut' style='left:{x * 100.0 / s.Width:F3}%'></span>");
            b.Append("</div><div class='parts'>");
            for (var i = 0; i < s.PartCount; i++)
                b.Append($"<div class='part' style=\"background-image:url('{H(uri)}');background-size:{s.PartCount * 100}% 100%;background-position:{(s.PartCount == 1 ? 0 : i * 100.0 / (s.PartCount - 1)):F2}% 0\"></div>");
            b.Append("</div></article>");
        }
        b.Append("</div><h2>Ürün planı</h2><table><tr><th>Kod</th><th>Ürün</th><th>Kaynak</th><th>Yeni görsel</th><th>Korunan</th><th>Kaldırılacak deneme.webp</th><th>Görselsiz riski</th></tr>");
        foreach (var p in r.Products) b.Append($"<tr><td>{H(p.ProductCode)}</td><td>{H(p.ProductName)}</td><td>{p.SourceCount}{(p.UsesFallback ? " (fallback)" : "")}</td><td>{p.PlannedImages.Count}</td><td>{p.PreservedImages.Count}</td><td>{p.PlaceholderRecordsToRemove}</td><td>{(p.CouldBecomeImageless ? "Evet" : "Hayır")}</td></tr>");
        b.Append("</table><h2>Eşleşmeyen / belirsiz kaynaklar</h2><table><tr><th>Dosya</th><th>Aday kod</th><th>Durum</th><th>Aday ürünler</th></tr>");
        foreach (var s in r.Sources.Where(x => x.MatchKind is "none" or "ambiguous")) b.Append($"<tr><td>{H(s.RelativePath)}</td><td>{H(s.CandidateCode)}</td><td>{H(s.MatchKind)}</td><td>{H(string.Join(", ", s.CandidateProductCodes))}</td></tr>");
        return b.Append("</table></body></html>").ToString();
    }

    [GeneratedRegex(@"^[A-Za-z0-9]+(?:[-_][A-Za-z0-9]+)*")]
    private static partial Regex LeadingCode();
    [GeneratedRegex(@"[-_\s]+")]
    private static partial Regex Separator();
    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex UnsafeName();
    [GeneratedRegex(@"(?i)(?:^|\s)2\s*(?:LI|LU|Lİ|LÜ)(?:\s|$)")]
    private static partial Regex DoubleMarker();
    [GeneratedRegex(@"(?i)(?:^|\s)3\s*(?:LI|LU|Lİ|LÜ)(?:\s|$)")]
    private static partial Regex TripleMarker();

    private sealed record ProductRecord(int Id, string Code, string? Name);
    private sealed record ExistingImage(int ProductId, string FilePath, bool IsPrimary, int DisplayOrder);
    private sealed record SplitDetection(int PartCount, string Confidence, string Reason, List<int> SplitXs);
}

public sealed class ImportReport
{
    public DateTime GeneratedAtUtc { get; set; }
    public string SourceRoot { get; set; } = "";
    public int DatabaseProductCount { get; set; }
    public int SourceImageCount { get; set; }
    public string ExistingStateFingerprint { get; set; } = "";
    public string? ReportToken { get; set; }
    public List<SourceAnalysis> Sources { get; set; } = [];
    public List<ProductPlan> Products { get; set; } = [];
    public int MatchedProductCount => Products.Count(p => p.SourceCount > 0);
    public int FallbackProductCount => Products.Count(p => p.UsesFallback);
    public int UnmatchedCount => Sources.Count(s => s.MatchKind == "none");
    public int AmbiguousMatchCount => Sources.Count(s => s.MatchKind == "ambiguous");
    public int SingleCount => Sources.Count(s => s.PartCount == 1 && s.SplitConfidence != "uncertain");
    public int DoubleCount => Sources.Count(s => s.PartCount == 2);
    public int TripleCount => Sources.Count(s => s.PartCount == 3);
    public int UncertainSplitCount => Sources.Count(s => s.SplitConfidence == "uncertain");
    public int PlannedImageCount => Products.Sum(p => p.PlannedImages.Count);
    public int PlaceholderRemovalCount => Products.Sum(p => p.PlaceholderRecordsToRemove);
    public bool HasBlockers => AmbiguousMatchCount > 0 || UncertainSplitCount > 0;
}

public sealed class SourceAnalysis
{
    public string RelativePath { get; set; } = "";
    public string FullPath { get; set; } = "";
    public long Length { get; set; }
    public long LastWriteUtcTicks { get; set; }
    public string? CandidateCode { get; set; }
    public string MatchKind { get; set; } = "none";
    public int? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public List<string> CandidateProductCodes { get; set; } = [];
    public int Width { get; set; }
    public int Height { get; set; }
    public int PartCount { get; set; }
    public string SplitConfidence { get; set; } = "uncertain";
    public string SplitReason { get; set; } = "";
    public List<int> SplitXs { get; set; } = [];
}

public sealed class ProductPlan
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string? ProductName { get; set; }
    public int SourceCount { get; set; }
    public List<PlannedImage> PlannedImages { get; set; } = [];
    public List<string> PreservedImages { get; set; } = [];
    public int PlaceholderRecordsToRemove { get; set; }
    public bool CouldBecomeImageless { get; set; }
    public bool UsesFallback { get; set; }
}

public sealed record PlannedImage(string SourceRelativePath, string SourceFullPath, int PartNumber, int PartCount, string TargetPath);
