using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.ViewModel;
using ToDoListManagement.Service.Helper;
using ToDoListManagement.Service.Interfaces;

namespace ToDoListManagement.Web.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IAuthService authService, IDashboardService dashboardService)
        : base(authService)
    {
        _dashboardService = dashboardService;
    }

    [CustomAuthorize("Projects", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (ViewBag.SessionUser == null || string.IsNullOrEmpty(ViewBag.SessionUser.Email))
        {
            return RedirectToAction("Login", "Auth");
        }
        DashboardViewModel model = await _dashboardService.GetDashboardDataAsync(ViewBag.SessionUser.UserId, ViewBag.SessionUser.Role);
        return View(model);
    }

    [CustomAuthorize("Projects", "CanView")]
    [HttpGet]
    public async Task<JsonResult> GetProjects(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
    {
        int pageNumber = start / length + 1;
        int pageSize = length;

        Pagination<ProjectViewModel>? pagination = new()
        {
            SearchKeyword = searchValue,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        if (SessionUser == null)
        {
            return Json(new
            {
                draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = Array.Empty<ProjectViewModel>()
            });
        }

        Pagination<ProjectViewModel>? data = await _dashboardService.GetPaginatedProjectsAsync(pagination, SessionUser);

        return Json(new
        {
            draw,
            recordsTotal = data.TotalRecords,
            recordsFiltered = data.TotalRecords,
            data = data.Items
        });
    }
}