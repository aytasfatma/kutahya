using Application.Dashboard;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Home;

namespace Presentation.Controllers;

/// <summary>Madde 17.2 modül #1 (Dashboard, Task 18) — dört rolün tamamı görüntüleyebilir (dokümanda
/// role göre farklılaşmaya dair kanıt yok, Task 17'nin yetki matrisiyle tutarlı). AntiForgery/PRG
/// gerekmiyor — yalnızca GET, state-changing action yok.</summary>
[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return View(new DashboardViewModel { Dashboard = dashboard });
    }

    [HttpGet]
    public async Task<IActionResult> Statistics()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return PartialView("_DashboardStatistics", new DashboardViewModel { Dashboard = dashboard });
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    public IActionResult AdminOnly()
    {
        return View();
    }
}
