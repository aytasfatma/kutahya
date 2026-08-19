using System.Security.Claims;
using Application.Users;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.User;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Yetkilendirme) Kullanıcı Yönetimi satırı: Admin=Tam, İçerik Editörü=—,
/// SEO Editörü=—, Ürün Yöneticisi=— — Dealer/Bayi-Showroom ile aynı salt-Admin desen, tek rol
/// sabiti tüm action'larda yeterli.</summary>
[Authorize(Roles = ApplicationRoles.Admin)]
public class UserController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UserController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>Mevcut oturumdaki kullanıcının Id'si — kendi-hesap guardrail'leri için (Task 16).
    /// UserManager'a bağımlılık eklemeden doğrudan claim'den okunuyor (AccountController'ın aksine
    /// bu controller yalnızca Application katmanı servisine bağımlı kalır).</summary>
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public async Task<IActionResult> Index()
    {
        var users = await _userManagementService.GetAllAsync();
        return View(new UserIndexViewModel { Users = users });
    }

    public async Task<IActionResult> Create()
    {
        var roles = await _userManagementService.GetAvailableRolesAsync();
        return View(new UserFormViewModel { IsActive = true, AvailableRoles = roles.ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "E-posta zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Parola zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = (await _userManagementService.GetAvailableRolesAsync()).ToList();
            return View(model);
        }

        var request = new CreateUserRequest
        {
            Email = model.Email,
            Password = model.Password!,
            Role = model.Role,
            IsActive = model.IsActive
        };

        var result = await _userManagementService.CreateAsync(request);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.AvailableRoles = (await _userManagementService.GetAvailableRolesAsync()).ToList();
            return View(model);
        }

        TempData["SuccessMessage"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManagementService.GetByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManagementService.GetAvailableRolesAsync();
        var model = new UserFormViewModel
        {
            Id = user.Id,
            Email = user.Email,
            IsActive = user.IsActive,
            Role = user.Role,
            AvailableRoles = roles.ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model)
    {
        // Password/ConfirmPassword Edit formunda render edilmiyor — DataAnnotations'ın Compare
        // kontrolü ikisi de boşken zaten geçer, ekstra bir ModelState temizliğine gerek yok.
        // Email de Edit formunda render edilmiyor (salt-okunur/değişmez, Task 16B revizyon kararı) —
        // [EmailAddress] boş değerde tetiklenmediği için ModelState'i etkilemez.
        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.AvailableRoles = (await _userManagementService.GetAvailableRolesAsync()).ToList();
            return View(model);
        }

        var request = new UpdateUserRequest
        {
            Role = model.Role,
            IsActive = model.IsActive
        };

        var result = await _userManagementService.UpdateAsync(id, request, CurrentUserId);

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            model.AvailableRoles = (await _userManagementService.GetAvailableRolesAsync()).ToList();
            return View(model);
        }

        TempData["SuccessMessage"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManagementService.GetByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ResetPasswordViewModel { Id = id, Email = user.Email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var result = await _userManagementService.ResetPasswordAsync(
            id, new ResetUserPasswordRequest { NewPassword = model.NewPassword });

        if (!result.Succeeded)
        {
            this.AddOperationError(result.ErrorMessage);
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Parola sıfırlandı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        var result = await _userManagementService.ActivateAsync(id);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Kullanıcı aktifleştirildi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var result = await _userManagementService.DeactivateAsync(id, CurrentUserId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Kullanıcı pasifleştirildi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userManagementService.DeleteAsync(id, CurrentUserId);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "Kullanıcı silindi." : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }
}

