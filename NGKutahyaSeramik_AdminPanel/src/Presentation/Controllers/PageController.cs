using Application.Pages;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Page;

namespace Presentation.Controllers;

/// <summary>
/// Madde 30 (Yetkilendirme) Sayfa Yönetimi satırı: Admin=Tam, İçerik Editörü=CRUD, SEO Editörü=Meta
/// Alanları, Ürün Yöneticisi=—. Backlog #23 (alan-seviyeli RBAC) ile SEO Editörü artık Edit action'ına
/// erişebiliyor — ama yalnızca SeoUrl/MetaTitle/MetaDescription'ı değiştirebilir (bkz.
/// PageFieldPermissions/RestrictToPermittedFields). Title ve içerik blokları (PageContentBlockController,
/// hâlâ yalnızca Admin+İçerik Editörü) SEO Editörü için salt-okunur kalır. Create bilinçli olarak
/// EditRoles'ta kalıyor — SEO Editörü yeni sayfa OLUŞTURAMAZ, yalnızca mevcut sayfanın SEO alanlarını
/// düzenleyebilir.
/// </summary>
[Authorize(Roles = PageController.ViewRoles)]
public class PageController : Controller
{
    private const string EditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;
    private const string FieldEditRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor + "," + ApplicationRoles.SeoEditor;

    private readonly PageService _pageService;
    private readonly PageContentBlockService _pageContentBlockService;

    public PageController(PageService pageService, PageContentBlockService pageContentBlockService)
    {
        _pageService = pageService;
        _pageContentBlockService = pageContentBlockService;
    }

    public async Task<IActionResult> Index()
    {
        var pages = await _pageService.GetAllAsync();
        return View(pages.OrderBy(p => p.DisplayTitle).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var page = await _pageService.GetByIdAsync(id);
        if (page is null)
        {
            TempData["ErrorMessage"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Blocks = await _pageContentBlockService.GetByPageIdAsync(id);
        return View(page);
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> Create()
    {
        var model = new PageFormViewModel
        {
            Translations = await BuildEmptyTranslationsAsync()
        };

        return View(model);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new CreatePageRequest
        {
            Translations = MapToTranslationInputs(model.Translations)
        };

        var result = await _pageService.CreateAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Sayfa oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = FieldEditRoles)]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await _pageService.GetByIdAsync(id);
        if (page is null)
        {
            TempData["ErrorMessage"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PageFormViewModel
        {
            Id = page.Id,
            Translations = page.Translations.Select(t => new PageTranslationFieldViewModel
            {
                LanguageId = t.LanguageId,
                LanguageCode = t.LanguageCode,
                LanguageName = t.LanguageName,
                Title = t.Title,
                SeoUrl = t.SeoUrl,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription
            }).ToList()
        };

        ApplyFieldPermissions(model);

        ViewBag.Blocks = await _pageContentBlockService.GetByPageIdAsync(id);
        return View(model);
    }

    [Authorize(Roles = FieldEditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageFormViewModel model)
    {
        var current = await _pageService.GetByIdAsync(id);
        if (current is null)
        {
            TempData["ErrorMessage"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ApplyFieldPermissions(model);
        RestrictToPermittedFields(model, current);

        if (!ModelState.IsValid)
        {
            model.Id = id;
            ViewBag.Blocks = await _pageContentBlockService.GetByPageIdAsync(id);
            return View(model);
        }

        var request = new UpdatePageRequest
        {
            Translations = MapToTranslationInputs(model.Translations)
        };

        var result = await _pageService.UpdateAsync(id, request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            ViewBag.Blocks = await _pageContentBlockService.GetByPageIdAsync(id);
            return View(model);
        }

        TempData["SuccessMessage"] = "Sayfa güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _pageService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Sayfa silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Backlog #23 — Admin/İçerik Editörü kısıtlanmaz; SEO Editörü yalnızca SEO alanlarına
    /// erişebilir (Title dahil hiçbir içerik alanına erişemez).</summary>
    private void ApplyFieldPermissions(PageFormViewModel model)
    {
        if (User.IsInRole(ApplicationRoles.Admin) || User.IsInRole(ApplicationRoles.ContentEditor))
        {
            return;
        }

        model.CanEditContentFields = false;
        model.CanEditSeoFields = User.IsInRole(ApplicationRoles.SeoEditor);
    }

    /// <summary>Backlog #23 — Overposting koruması: SEO Editörü için POST edilen Title değerleri
    /// yok sayılır, DB'den taze okunan mevcut değerle geri yazılır. Yalnızca View'da disabled yapmak
    /// yeterli değildir — bu yüzden Controller'da da zorunlu kontrol.</summary>
    private void RestrictToPermittedFields(PageFormViewModel model, PageDto current)
    {
        if (User.IsInRole(ApplicationRoles.Admin) || User.IsInRole(ApplicationRoles.ContentEditor))
        {
            return;
        }

        var allowedFields = User.IsInRole(ApplicationRoles.SeoEditor)
            ? PageFieldPermissions.SeoEditorFields
            : [];

        foreach (var translation in model.Translations)
        {
            var currentTranslation = current.Translations.FirstOrDefault(t => t.LanguageId == translation.LanguageId);
            if (currentTranslation is null)
            {
                continue;
            }

            if (!allowedFields.Contains(PageFields.Title))
            {
                translation.Title = currentTranslation.Title;
            }

            if (!allowedFields.Contains(PageFields.SeoUrl))
            {
                translation.SeoUrl = currentTranslation.SeoUrl;
            }

            if (!allowedFields.Contains(PageFields.MetaTitle))
            {
                translation.MetaTitle = currentTranslation.MetaTitle;
            }

            if (!allowedFields.Contains(PageFields.MetaDescription))
            {
                translation.MetaDescription = currentTranslation.MetaDescription;
            }
        }
    }

    private static IReadOnlyList<PageTranslationInput> MapToTranslationInputs(
        IEnumerable<PageTranslationFieldViewModel> translations) =>
        translations.Select(t => new PageTranslationInput
        {
            LanguageId = t.LanguageId,
            Title = t.Title,
            SeoUrl = t.SeoUrl,
            MetaTitle = t.MetaTitle,
            MetaDescription = t.MetaDescription
        }).ToList();

    private async Task<List<PageTranslationFieldViewModel>> BuildEmptyTranslationsAsync()
    {
        var languages = await _pageService.GetActiveLanguagesAsync();

        return languages.Select(l => new PageTranslationFieldViewModel
        {
            LanguageId = l.Id,
            LanguageCode = l.Code,
            LanguageName = l.Name
        }).ToList();
    }
}

