using Application.Roles;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Role;

namespace Presentation.Controllers;

/// <summary>Madde 30 (Kullanıcı/Rol Yönetimi, Admin-only). Task 17 kapsamı: salt-okunur — yalnızca
/// Index/Details GET action'ları var, hiçbir state-changing action yok (rol oluşturma/silme/yeniden
/// adlandırma/kullanıcıya rol atama kasıtlı olarak bu modülde değil). AntiForgery/PRG gerektirmiyor.</summary>
[Authorize(Roles = ApplicationRoles.Admin)]
public class RoleController : Controller
{
    private readonly IRoleManagementService _roleManagementService;

    public RoleController(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManagementService.GetAllAsync();
        return View(new RoleIndexViewModel { Roles = roles });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var role = await _roleManagementService.GetByNameAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        return View(new RoleDetailViewModel { Role = role });
    }
}
