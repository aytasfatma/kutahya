using Application.Dealers;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class DealerImageController : Controller
{
    private const long MaxUploadBytes = 5 * 1024 * 1024;
    private readonly DealerImageService _service;
    public DealerImageController(DealerImageService service) => _service = service;

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Upload(int dealerId, IFormFile? file)
    {
        if (file is null || file.Length == 0) return Back(dealerId, false, "Lütfen bir görsel seçin.");
        await using var stream = file.OpenReadStream();
        var result = await _service.AddAsync(new AddDealerImageRequest { DealerId = dealerId, OriginalFileName = file.FileName, ContentType = file.ContentType, Length = file.Length, Content = stream });
        return Back(dealerId, result.Succeeded, result.Succeeded ? "Görsel yüklendi." : result.ErrorMessage!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFeatured(int dealerId, int imageId) { var result = await _service.SetFeaturedAsync(dealerId, imageId); return Back(dealerId, result.Succeeded, result.Succeeded ? "Kapak görseli güncellendi." : result.ErrorMessage!); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int dealerId, int imageId) { var result = await _service.MoveUpAsync(dealerId, imageId); return Back(dealerId, result.Succeeded, result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage!); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int dealerId, int imageId) { var result = await _service.MoveDownAsync(dealerId, imageId); return Back(dealerId, result.Succeeded, result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage!); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int dealerId, int imageId) { var result = await _service.DeleteAsync(dealerId, imageId); return Back(dealerId, result.Succeeded, result.Succeeded ? "Görsel silindi." : result.ErrorMessage!); }

    private IActionResult Back(int dealerId, bool succeeded, string message)
    {
        TempData[succeeded ? "SuccessMessage" : "ErrorMessage"] = message;
        return RedirectToAction("Images", "Dealer", new { id = dealerId });
    }
}
