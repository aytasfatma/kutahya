using Application.Dealers;
using Application.Forms;
using Application.Languages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presentation.Models.Api;

namespace Presentation.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicOperationsController : ControllerBase
{
    private readonly DealerService _dealerService;
    private readonly FormSubmissionService _formSubmissionService;
    private readonly LanguageService _languageService;
    private readonly DealerImageService? _dealerImageService;
    private readonly NotificationSettingsService? _notificationSettingsService;
    private readonly IEmailNotificationService? _emailNotificationService;

    public PublicOperationsController(
        DealerService dealerService,
        FormSubmissionService formSubmissionService,
        LanguageService languageService,
        DealerImageService? dealerImageService = null,
        NotificationSettingsService? notificationSettingsService = null,
        IEmailNotificationService? emailNotificationService = null)
    {
        _dealerService = dealerService;
        _formSubmissionService = formSubmissionService;
        _languageService = languageService;
        _dealerImageService = dealerImageService;
        _notificationSettingsService = notificationSettingsService;
        _emailNotificationService = emailNotificationService;
    }

    [HttpGet("dealers")]
    public async Task<ActionResult<IReadOnlyList<PublicDealerResponse>>> Dealers(
        [FromQuery] string? city = null, [FromQuery] Domain.Enums.DealerCategory? category = null,
        [FromQuery] Domain.Enums.ProductBrand? brand = null)
    {
        var source = (await _dealerService.GetAllAsync())
            .Where(x => x.IsActive)
            .Where(x => string.IsNullOrWhiteSpace(city)
                || string.Equals(x.City, city.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(x => !category.HasValue || x.Category == category)
            .Where(x => !brand.HasValue || x.Brands.Contains(brand.Value))
            .OrderBy(x => x.City)
            .ThenBy(x => x.Name).ToList();
        var dealers = new List<PublicDealerResponse>(source.Count);
        foreach (var dealer in source) dealers.Add(await MapDealerAsync(dealer));
        return Ok(dealers);
    }

    [HttpGet("dealers/{id:int}")]
    public async Task<ActionResult<PublicDealerResponse>> Dealer(int id)
    {
        var dealer = await _dealerService.GetByIdAsync(id);
        return dealer is null || !dealer.IsActive ? NotFound() : Ok(await MapDealerAsync(dealer));
    }

    [HttpGet("languages")]
    public async Task<ActionResult<IReadOnlyList<PublicLanguageResponse>>> Languages()
    {
        var languages = (await _languageService.GetAllAsync())
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new PublicLanguageResponse(x.Id, x.Code.ToLowerInvariant(), x.Name, x.DisplayOrder))
            .ToList();
        return Ok(languages);
    }

    [HttpPost("forms")]
    [EnableRateLimiting("public-forms")]
    [ProducesResponseType<PublicFormSubmissionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PublicFormSubmissionResponse>> SubmitForm(PublicFormSubmissionRequest request)
    {
        if (!Enum.TryParse<Domain.Enums.FormType>(request.FormType, ignoreCase: true, out var formType))
        {
            ModelState.AddModelError(nameof(request.FormType),
                "Form türü Contact, RequestInformation veya SampleRequest olmalıdır.");
            return ValidationProblem(ModelState);
        }

        var result = await _formSubmissionService.CreateSubmissionAsync(new CreateFormSubmissionRequest
        {
            FormType = formType, FullName = request.FullName, Email = request.Email,
            Phone = request.Phone, Company = request.Company, Message = request.Message,
            ConsentAccepted = request.ConsentAccepted, Subject = request.Subject,
            ProductCode = request.ProductCode, ProductName = request.ProductName,
            Address = request.Address, RequestedProduct = request.RequestedProduct, Quantity = request.Quantity
        });

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Form gönderilemedi.");
            return ValidationProblem(ModelState);
        }

        return StatusCode(StatusCodes.Status201Created,
            new PublicFormSubmissionResponse(true, "Formunuz başarıyla alındı."));
    }

    [HttpPost("forms/career")]
    [EnableRateLimiting("public-forms")]
    [RequestSizeLimit(5 * 1024 * 1024 + 16 * 1024)]
    public async Task<ActionResult<PublicFormSubmissionResponse>> SubmitCareer([FromForm] PublicCareerSubmissionRequest request)
    {
        var cvError = await ValidateCvAsync(request.Cv); if (cvError is not null) return BadRequest(new PublicFormSubmissionResponse(false, cvError));
        var result = await _formSubmissionService.CreateSubmissionAsync(new CreateFormSubmissionRequest
        {
            FormType = Domain.Enums.FormType.Contact, FullName = request.FullName, Email = request.Email,
            Phone = request.Phone, Message = request.Message, ConsentAccepted = request.ConsentAccepted,
            Subject = $"Kariyer / {request.Department}"
        });
        if (!result.Succeeded) return BadRequest(new PublicFormSubmissionResponse(false, result.ErrorMessage ?? "Başvuru kaydedilemedi."));

        var settings = _notificationSettingsService is null ? new NotificationSettingsDto("mratdrn@gmail.com", true) : await _notificationSettingsService.GetAsync();
        if (!settings.CareerEmailEnabled) return StatusCode(201, new PublicFormSubmissionResponse(true, "Başvurunuz kaydedildi."));
        try
        {
            byte[]? bytes = null; if (request.Cv is not null) { await using var stream = request.Cv.OpenReadStream(); using var memory = new MemoryStream(); await stream.CopyToAsync(memory); bytes = memory.ToArray(); }
            if (_emailNotificationService is null) throw new InvalidOperationException("E-posta servisi hazır değil.");
            await _emailNotificationService.SendCareerApplicationAsync(new CareerEmailRequest(settings.CareerRecipientEmail,
                request.FullName, request.Email, request.Phone, request.Department, request.Message,
                request.Cv?.FileName, request.Cv?.ContentType, bytes));
            return StatusCode(201, new PublicFormSubmissionResponse(true, "Başvurunuz alındı ve İnsan Kaynakları'na gönderildi."));
        }
        catch
        {
            return StatusCode(201, new PublicFormSubmissionResponse(true, "Başvurunuz kaydedildi; e-posta gönderimi SMTP ayarları tamamlandığında etkinleşecek."));
        }
    }

    private async Task<PublicDealerResponse> MapDealerAsync(DealerDto x)
    {
        var images = _dealerImageService is null ? [] : await _dealerImageService.GetByDealerIdAsync(x.Id);
        var mapped = images.Select(i => new PublicDealerImageResponse(i.Id, AssetUrl(i.FilePath), i.IsFeatured, i.DisplayOrder)).ToList();
        return new PublicDealerResponse(
        x.Id, x.Name, x.City, x.District, x.Address, x.Phone, x.Email, x.WorkingHours,
        x.Category?.ToString() ?? string.Empty, x.CategoryLabel, x.Latitude, x.Longitude,
        x.Region, x.RegionName, mapped.FirstOrDefault(i => i.IsFeatured)?.Url, mapped,
        x.Brands.Select(brand => brand.ToString()).ToList());
    }

    private string AssetUrl(string path)
    {
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{path.TrimStart('/')}";
    }

    private static async Task<string?> ValidateCvAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return "CV dosyası zorunludur.";
        if (file.Length > 5 * 1024 * 1024) return "CV dosyası en fazla 5 MB olabilir.";
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".pdf" or ".doc" or ".docx")) return "CV yalnızca PDF, DOC veya DOCX olabilir.";
        await using var stream = file.OpenReadStream(); var header = new byte[8]; var read = await stream.ReadAsync(header);
        var valid = extension switch { ".pdf" => read >= 4 && header[0] == '%' && header[1] == 'P' && header[2] == 'D' && header[3] == 'F', ".docx" => read >= 4 && header[0] == 'P' && header[1] == 'K' && header[2] == 3 && header[3] == 4, ".doc" => read >= 8 && header.SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }), _ => false };
        return valid ? null : "CV dosyasının içeriği uzantısıyla uyuşmuyor.";
    }
}
