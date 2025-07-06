using Microsoft.AspNetCore.Mvc;
using ToDoListManagement.Entity.Constants;
using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.ViewModel;
using ToDoListManagement.Service.Helper;
using ToDoListManagement.Service.Interfaces;

namespace ToDoListManagement.Web.Controllers;

public class ProjectController : BaseController
{
    private readonly IProjectService _projectService;
    public ProjectController(IAuthService authService, IProjectService projectService) : base(authService)
    {
        _projectService = projectService;
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> AddProject()
    {
        List<UserViewModel> projectManagers = await _projectService.GetProjectManagersAsync();
        List<UserViewModel> scrumMasters = await _projectService.GetScrumMastersAsync();
        ProjectViewModel model = new()
        {
            ProjectManagers = projectManagers,
            ScrumMasters = scrumMasters
        };
        
        return PartialView("~/Views/Dashboard/_AddProjectModal.cshtml", model);
    }

    [CustomAuthorize("Projects", "CanView")]
    public async Task<IActionResult> CheckProjectNameExists(string projectName, int projectId = 0)
    {
        await Task.Delay(1000);
        bool exists = await _projectService.CheckProjectNameExistsAsync(projectName.Trim(), projectId);
        return Json(!exists);
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> AddProject(ProjectViewModel model)
    {
        if (SessionUser == null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        bool isAdded = await _projectService.AddProject(model, SessionUser.UserId);
        if (isAdded)
        {
            TempData["SuccessMessage"] = Constants.ProjectAddedMessage;
        }
        else
        {
            TempData["ErrorMessage"] = Constants.ProjectAddFailedMessage;
        }
        return RedirectToAction("Index", "Dashboard");
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> GetProjectById(int projectId)
    {
        ProjectViewModel? model = await _projectService.GetProjectByIdAsync(projectId);
        if (model == null)
        {
            return Json(new { success = false, message = Constants.ProjectNotFoundMessage });
        }
    
        return PartialView("~/Views/Dashboard/_UpdateProjectModal.cshtml", model);
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> UpdateProject(ProjectViewModel model)
    {
        bool isUpdated = await _projectService.UpdateProjectAsync(model);
        if (isUpdated)
        {
            TempData["SuccessMessage"] = Constants.ProjectUpdatedMessage;
        }
        else
        {
            TempData["ErrorMessage"] = Constants.ProjectUpdateFailedMessage;
        }
        return RedirectToAction("Index", "Dashboard");
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> GetAssignedMembers(int projectId, int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
    {
        int pageNumber = start / length + 1;
        int pageSize = length;

        Pagination<MemberViewModel>? pagination = new()
        {
            SearchKeyword = searchValue,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
        
        Pagination<MemberViewModel> data = await _projectService.GetAssignedMembersAsync(pagination, projectId);

        return Json(new
        {
            draw,
            recordsTotal = data.TotalRecords,
            recordsFiltered = data.TotalRecords,
            data = data.Items
        });
    }

    [CustomAuthorize("Projects", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> AssignMembers([FromBody] AssignMembersViewModel request)
    {
        bool isAssigned =  await _projectService.AssignMembersAsync(request.ProjectId, request.UserIds);
        if(isAssigned)
        {
            return Json(new { success = true, message = Constants.MembersAssignedMessage });
        }
        else
        {
            return Json(new { success = false, message = Constants.MembersAssignedFailedMessage });
        }
    }

    [CustomAuthorize("Projects", "CanDelete")]
    [HttpGet]
    public async Task<IActionResult> DeleteProject(int projectId)
    {
        if (SessionUser == null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        bool isDeleted = await _projectService.DeleteProjectAsync(projectId, SessionUser.UserId);
        if (isDeleted)
        {
            TempData["SuccessMessage"] = Constants.ProjectDeletedMessage;
        }
        else
        {
            TempData["ErrorMessage"] = Constants.ProjectDeleteFailedMessage;
        }
        
        return RedirectToAction("Index", "Dashboard");
    }
}