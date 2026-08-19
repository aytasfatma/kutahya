using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Account;

namespace Presentation.Controllers;

public class AccountController : Controller
{
    private const string InvalidLoginMessage = "Geçersiz e-posta veya parola.";

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Task 16: Pasif (IsActive=false) kullanıcı, doğru parola girse bile giriş yapamaz. Kontrol
        // PasswordSignInAsync'ten ÖNCE yapılır — böylece pasif bir hesap için AccessFailedCount/lockout
        // sayaçları anlamsız yere işlemez ve "kullanıcı yok" ile "kullanıcı pasif" durumları dışarıya
        // aynı genel hata mesajıyla yansır (bilgi sızıntısı yok).
        if (user is not null && user.IsActive)
        {
            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Bir giriş denemesi hesap kilitlenmesi nedeniyle engellendi.");
            }
        }

        ModelState.AddModelError(string.Empty, InvalidLoginMessage);
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
