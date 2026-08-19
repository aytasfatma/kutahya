using Application.ProductImages;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize(Roles = EditRoles)]
public class ProductImageController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ProductManager;

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private readonly ProductImageService _productImageService;

    public ProductImageController(ProductImageService productImageService)
    {
        _productImageService = productImageService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadBytes + 4096)]
    public async Task<IActionResult> Upload(int productId, ProductImageType imageType, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Lütfen bir görsel dosyası seçin.";
            return RedirectToAction("Edit", "Product", new { id = productId });
        }

        await using var stream = file.OpenReadStream();
        var request = new AddProductImageRequest
        {
            ProductId = productId,
            ImageType = imageType,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = stream
        };

        var result = await _productImageService.AddImageAsync(request);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Görsel eklendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimary(int productId, int imageId)
    {
        var result = await _productImageService.SetPrimaryAsync(productId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Ana görsel güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int productId, int imageId)
    {
        var result = await _productImageService.MoveUpAsync(productId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int productId, int imageId)
    {
        var result = await _productImageService.MoveDownAsync(productId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sıralama güncellendi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId, int imageId)
    {
        var result = await _productImageService.DeleteImageAsync(productId, imageId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Görsel silindi." : result.ErrorMessage;

        return RedirectToAction("Edit", "Product", new { id = productId });
    }
}
