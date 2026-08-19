using Application.ReferenceProjects;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize(Roles = EditRoles)]
public class ReferenceProjectImageController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly ReferenceProjectImageService _referenceProjectImageService;

    public ReferenceProjectImageController(ReferenceProjectImageService referenceProjectImageService)
    {
        _referenceProjectImageService = referenceProjectImageService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Upload(int referenceProjectId, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Lütfen bir görsel dosyası seçin.";
            return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
        }

        await using var stream = file.OpenReadStream();
        var request = new AddReferenceProjectImageRequest
        {
            ReferenceProjectId = referenceProjectId,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = stream
        };

        var result = await _referenceProjectImageService.AddImageAsync(request);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Görsel eklendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFeatured(int referenceProjectId, int imageId)
    {
        var result = await _referenceProjectImageService.SetFeaturedAsync(referenceProjectId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Kapak görseli güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int referenceProjectId, int imageId)
    {
        var result = await _referenceProjectImageService.MoveUpAsync(referenceProjectId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int referenceProjectId, int imageId)
    {
        var result = await _referenceProjectImageService.MoveDownAsync(referenceProjectId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int referenceProjectId, int imageId)
    {
        var result = await _referenceProjectImageService.DeleteImageAsync(referenceProjectId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Görsel silindi." : result.ErrorMessage;

        return RedirectToAction("Edit", "ReferenceProject", new { id = referenceProjectId });
    }
}
