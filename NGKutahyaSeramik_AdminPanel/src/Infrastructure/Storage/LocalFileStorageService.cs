using Application.Storage;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Storage;

/// <summary>
/// ADR-006 — Yerel Dosya Sistemi + storage abstraction. Fiziksel kök: wwwroot/uploads.
/// İleride Blob/S3/MinIO'ya geçiş bu sınıf değiştirilerek yapılabilir; Application/Presentation
/// katmanları IFileStorageService interface'i üzerinden çalıştığı için etkilenmez.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _webRootPath = environment.WebRootPath;
    }

    public async Task<string> SaveAsync(string relativeFolder, Stream content, string fileName)
    {
        var safeFolder = NormalizeFolder(relativeFolder);
        var physicalFolder = Path.Combine(_webRootPath, "uploads", safeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalFolder);

        var physicalPath = Path.Combine(physicalFolder, fileName);
        EnsureWithinFolder(physicalPath, physicalFolder);

        using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream);
        }

        return $"/uploads/{safeFolder}/{fileName}";
    }

    public void Delete(string relativeFilePath)
    {
        var physicalPath = ResolvePhysicalPath(relativeFilePath);
        if (physicalPath is not null && File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }

    public bool Exists(string relativeFilePath)
    {
        var physicalPath = ResolvePhysicalPath(relativeFilePath);
        return physicalPath is not null && File.Exists(physicalPath);
    }

    public Task<Stream> OpenReadAsync(string relativeFilePath)
    {
        var physicalPath = ResolvePhysicalPath(relativeFilePath);
        if (physicalPath is null || !File.Exists(physicalPath))
        {
            throw new FileNotFoundException("Dosya bulunamadı.", relativeFilePath);
        }

        Stream stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    private static string NormalizeFolder(string relativeFolder) =>
        relativeFolder.Replace('\\', '/').Trim('/');

    private static void EnsureWithinFolder(string physicalPath, string physicalFolder)
    {
        var resolvedFull = Path.GetFullPath(physicalPath);
        var resolvedFolder = Path.GetFullPath(physicalFolder);

        if (!resolvedFull.StartsWith(resolvedFolder, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Geçersiz dosya yolu.");
        }
    }

    private string? ResolvePhysicalPath(string relativeFilePath)
    {
        if (string.IsNullOrWhiteSpace(relativeFilePath) || !relativeFilePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var trimmed = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_webRootPath, trimmed);

        var uploadsRoot = Path.GetFullPath(Path.Combine(_webRootPath, "uploads"));
        var resolvedFull = Path.GetFullPath(physicalPath);

        return resolvedFull.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) ? resolvedFull : null;
    }
}
