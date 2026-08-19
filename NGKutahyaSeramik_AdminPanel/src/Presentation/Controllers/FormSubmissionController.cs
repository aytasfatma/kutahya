using Application.Forms;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.FormSubmission;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Form Yönetimi satırı: Admin=Tam, İçerik Editörü=Görüntüleme,
/// SEO Editörü=—, Ürün Yöneticisi=—. İçerik Editörü yalnızca listeleme/detay görebilir; okundu
/// işaretleme, not ekleme, silme yalnızca Admin'e açık.</summary>
[Authorize(Roles = FormSubmissionController.ViewRoles)]
public class FormSubmissionController : Controller
{
    internal const string ViewRoles = ApplicationRoles.Admin + "," + ApplicationRoles.ContentEditor;
    private const string EditRoles = ApplicationRoles.Admin;

    private const int DefaultPageSize = 20;

    private readonly FormSubmissionService _formSubmissionService;
    private readonly NotificationSettingsService? _notificationSettingsService;

    public FormSubmissionController(FormSubmissionService formSubmissionService, NotificationSettingsService? notificationSettingsService = null)
    {
        _formSubmissionService = formSubmissionService;
        _notificationSettingsService = notificationSettingsService;
    }

    [Authorize(Roles = EditRoles)]
    public async Task<IActionResult> EmailSettings()
    {
        if (_notificationSettingsService is null) return RedirectToAction(nameof(Index));
        return View(await _notificationSettingsService.GetAsync());
    }

    [Authorize(Roles = EditRoles), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EmailSettings(string careerRecipientEmail, bool careerEmailEnabled)
    {
        try { await _notificationSettingsService!.UpdateAsync(careerRecipientEmail, careerEmailEnabled); TempData["SuccessMessage"] = "E-posta ayarları güncellendi."; }
        catch (ArgumentException ex) { TempData["ErrorMessage"] = ex.Message; }
        return RedirectToAction(nameof(EmailSettings));
    }

    public async Task<IActionResult> Index(
        FormType? formType,
        bool? isRead,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? searchTerm,
        int page = 1)
    {
        var query = new FormSubmissionQuery
        {
            FormType = formType,
            IsRead = isRead,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            SearchTerm = searchTerm,
            PageNumber = page,
            PageSize = DefaultPageSize
        };

        var result = await _formSubmissionService.GetPagedAsync(query);

        var model = new FormSubmissionIndexViewModel
        {
            Page = result,
            FormType = formType,
            IsRead = isRead,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            SearchTerm = searchTerm
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        // Bilinçli olarak GET action içinde yan etki (otomatik okundu-işaretleme) YAPILMIYOR —
        // GET idempotent kalmalı ve "Görüntüleme" yetkisindeki İçerik Editörü'nün dolaylı bir yazma
        // işlemi tetiklemesi RBAC sınırını (Madde 30: İçerik Editörü=Görüntüleme, yazma yetkisi yok)
        // ihlal eder. Okundu işaretleme yalnızca Admin'in kullanabildiği ayrı bir POST action'dır.
        var submission = await _formSubmissionService.GetByIdAsync(id);
        if (submission is null)
        {
            TempData["ErrorMessage"] = "Form başvurusu bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(submission);
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _formSubmissionService.MarkAsReadAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Okundu olarak işaretlendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        var result = await _formSubmissionService.MarkAsUnreadAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Okunmadı olarak işaretlendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsProcessed(int id)
    {
        var result = await _formSubmissionService.MarkAsProcessedAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "İşleme alındı olarak işaretlendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsUnprocessed(int id)
    {
        var result = await _formSubmissionService.MarkAsUnprocessedAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "İşleme alınmadı olarak işaretlendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAdminNote(AdminNoteViewModel model)
    {
        var result = await _formSubmissionService.UpdateAdminNoteAsync(model.Id, model.AdminNote);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Not güncellendi." : result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [Authorize(Roles = EditRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _formSubmissionService.DeleteAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Form başvurusu silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }
}
