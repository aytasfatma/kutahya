using Application.Common;
using Application.Dealers;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Models.Dealer;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Bayi/Showroom satırı: Admin=Tam, İçerik Editörü=—, SEO Editörü=—,
/// Ürün Yöneticisi=— — projedeki ilk modül: yalnızca Admin'in HERHANGİ bir erişimi var, diğer üç rolün
/// salt-görüntüleme dahil hiçbir yetkisi yok. Bu nedenle tek bir rol sabiti (Admin) tüm action'larda
/// kullanılıyor, ayrı ViewRoles/EditRoles ayrımına gerek kalmadı.</summary>
[Authorize(Roles = ApplicationRoles.Admin)]
public class DealerController : Controller
{
    private readonly DealerService _dealerService;
    private readonly DealerImageService? _dealerImageService;

    public DealerController(DealerService dealerService, DealerImageService? dealerImageService = null)
    {
        _dealerService = dealerService;
        _dealerImageService = dealerImageService;
    }

    public async Task<IActionResult> Index(DealerCategory? category, string? city, string? status, string? search)
    {
        var isActive = status?.ToLowerInvariant() switch
        {
            "active" => true,
            "passive" => false,
            _ => (bool?)null
        };

        var dealers = await _dealerService.GetFilteredAsync(new DealerQuery
        {
            Category = category,
            City = city,
            IsActive = isActive,
            SearchTerm = search
        });

        ViewBag.SelectedCategory = category;
        ViewBag.SelectedCity = city;
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = search;
        ViewBag.Cities = TurkeyProvinces.All;

        return View(dealers);
    }

    public IActionResult Create(DealerCategory? category)
    {
        category ??= DealerCategory.SalesPoint;
        var model = new DealerFormViewModel
        {
            Category = category,
            CategoryOptions = BuildCategoryOptions(category)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DealerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = BuildCategoryOptions(model.Category);
            return View(model);
        }

        var request = new CreateDealerRequest
        {
            Name = model.Name,
            City = model.City,
            Category = model.Category,
            District = model.District,
            Address = model.Address,
            Phone = model.Phone,
            Fax = model.Fax,
            Email = model.Email,
            WorkingHours = model.WorkingHours,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Region = null,
            RegionName = null,
            Brands = model.Brands
        };

        var result = await _dealerService.CreateAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.CategoryOptions = BuildCategoryOptions(model.Category);
            return View(model);
        }

        TempData["SuccessMessage"] = $"{GetRecordLabel(model.Category)} kaydı oluşturuldu.";
        return RedirectToAction(nameof(Index), new { category = model.Category });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dealer = await _dealerService.GetByIdAsync(id);
        if (dealer is null)
        {
            TempData["ErrorMessage"] = "Bayi/showroom kaydı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new DealerFormViewModel
        {
            Id = dealer.Id,
            Name = dealer.Name,
            City = dealer.City,
            Category = dealer.Category,
            District = dealer.District,
            Address = dealer.Address,
            Phone = dealer.Phone,
            Fax = dealer.Fax,
            Email = dealer.Email,
            WorkingHours = dealer.WorkingHours,
            Latitude = dealer.Latitude,
            Longitude = dealer.Longitude,
            Brands = dealer.Brands.ToList(),
            CategoryOptions = BuildCategoryOptions(dealer.Category)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DealerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.CategoryOptions = BuildCategoryOptions(model.Category);
            return View(model);
        }

        var request = new UpdateDealerRequest
        {
            Name = model.Name,
            City = model.City,
            Category = model.Category,
            District = model.District,
            Address = model.Address,
            Phone = model.Phone,
            Fax = model.Fax,
            Email = model.Email,
            WorkingHours = model.WorkingHours,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Region = null,
            RegionName = null,
            Brands = model.Brands
        };

        var result = await _dealerService.UpdateAsync(id, request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            model.CategoryOptions = BuildCategoryOptions(model.Category);
            return View(model);
        }

        TempData["SuccessMessage"] = $"{GetRecordLabel(model.Category)} kaydı güncellendi.";
        return RedirectToAction(nameof(Index), new { category = model.Category });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var dealer = await _dealerService.GetByIdAsync(id);
        var result = await _dealerService.ToggleActiveAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? $"{GetRecordLabel(dealer?.Category)} durumu güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index), new { category = dealer?.Category });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var dealer = await _dealerService.GetByIdAsync(id);
        if (_dealerImageService is not null) await _dealerImageService.DeleteAllAsync(id);
        var result = await _dealerService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? $"{GetRecordLabel(dealer?.Category)} kaydı silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index), new { category = dealer?.Category });
    }

    public async Task<IActionResult> Images(int id)
    {
        var dealer = await _dealerService.GetByIdAsync(id);
        if (dealer is null || _dealerImageService is null)
        {
            TempData["ErrorMessage"] = "Satış noktası bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Dealer = dealer;
        return View(await _dealerImageService.GetByDealerIdAsync(id));
    }

    private static List<SelectListItem> BuildCategoryOptions(DealerCategory? selected)
    {
        var categoryOrder = new[]
        {
            DealerCategory.SalesPoint,
            DealerCategory.Factory,
            DealerCategory.GeneralHeadquarters
        };

        var options = categoryOrder
            .Select(c => new SelectListItem(DealerEnumDisplay.GetCategoryLabel(c), c.ToString(), selected == c));
        return options.ToList();
    }

    private static string GetRecordLabel(DealerCategory? category) => category switch
    {
        DealerCategory.GeneralHeadquarters => "Genel merkez",
        DealerCategory.Factory => "Tesis",
        DealerCategory.SalesPoint => "Satış noktası",
        _ => "Kayıt"
    };
}

