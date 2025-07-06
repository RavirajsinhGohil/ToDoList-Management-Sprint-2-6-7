using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ToDoListManagement.Entity.Constants;
using ToDoListManagement.Entity.ViewModel;
using ToDoListManagement.Service.Helper;
using ToDoListManagement.Service.Interfaces;
using ToDoListManagement.Web.Hub;

namespace ToDoListManagement.Web.Controllers;

[Authorize]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IProjectService _projectService;
    private readonly ISprintService _sprintService;

    public TasksController(IAuthService authService, ITaskService taskService, IHubContext<ChatHub> hubContext, IProjectService projectService, ISprintService sprintService)
        : base(authService)
    {
        _taskService = taskService;
        _hubContext = hubContext;
        _projectService = projectService;
        _sprintService = sprintService;
    }

    [CustomAuthorize("Task Board", "CanView")]
    [HttpGet]
    public async Task<IActionResult> Index(int? projectId)
    {
        if (SessionUser != null)
        {
            ProjectTasksViewModel model = await _taskService.GetProjectNamesAsync(SessionUser);
            model.ProjectId = projectId ?? 0;
            if (model.ProjectId != 0)
            {
                model.BacklogTasks = await _taskService.GetBacklogTasksByProjectIdAsync(model.ProjectId);
                model.Sprints = await _sprintService.GetAllSprints(model.ProjectId);
                model.TeamMembers = await _taskService.GetTeamMembersAsync(model.ProjectId);
            }
            return View(model);
        }
        return RedirectToAction("Login", "Auth");
    }

    [CustomAuthorize("Task Board", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetProjectsJson()
    {
        ProjectTasksViewModel model = await _taskService.GetProjectNamesAsync(SessionUser);
        return Json(model.Projects);
    }

    [CustomAuthorize("Task Board", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetTasksBySprintId(int sprintId, int userId )
    {
        List<TaskViewModel>? tasks = await _taskService.GetTasksBySprintIdAsync(sprintId, userId);
        return PartialView("_TasksList", tasks ?? []);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int taskId, string newStatus)
    {
        bool result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus);
        if (result)
        {
            return Json(new { success = true, message = Constants.TaskStausChangeMessage });
        }
        return Json(new { success = false, message = Constants.TaskStausChangeFailedMessage });
    }

    [CustomAuthorize("Task Board", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetSprintsJson(int projectId)
    {
        List<SprintViewModel> sprints = await _sprintService.GetAllSprints(projectId);
        return Json(sprints);
    }

    [CustomAuthorize("Task Board", "CanView")]
    [HttpGet]
    public async Task<IActionResult> GetTeamMembersJson(int projectId)
    {
        List<UserViewModel> teamMembers = await _taskService.GetTeamMembersAsync(projectId);
        return Json(teamMembers);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> AddTask(int projectId)
    {
        ProjectViewModel? project = await _projectService.GetProjectByIdAsync(projectId);

        TaskViewModel task = new()
        {
            ProjectId = projectId,
            ProjectStartDate = project?.StartDate ?? DateTime.UtcNow,
            ProjectEndDate = project?.EndDate ?? DateTime.UtcNow,
            TeamMembers = SessionUser != null ? await _taskService.GetTeamMembersAsync(projectId) : [],
            Sprints = await _sprintService.GetAllSprints(projectId)
        };
        return PartialView("_AddTaskModal", task);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    public async Task<IActionResult> CheckTaskTitleExists(string title, int taskId = 0, int projectId = 0)
    {
        await Task.Delay(1000);
        bool exists = await _taskService.CheckTaskNameExists(title.Trim(), taskId, projectId);
        return Json(!exists);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> AddTask(TaskViewModel model)
    {
        if (SessionUser?.UserId == null)
        {
            TempData["ErrorMessage"] = Constants.InvalidSessionMessage;
            return RedirectToAction("Index", new { projectId = model.ProjectId });
        }

        bool isAdded = await _taskService.AddTaskAsync(model, SessionUser);
        if (isAdded)
        {
            TempData["SuccessMessage"] = Constants.TaskAddedMessage;
            await _hubContext.Clients.All.SendAsync("NewTaskAdded");
        }
        else
        {
            TempData["ErrorMessage"] = Constants.TaskAddFailedMessage;
        }
        return RedirectToAction("Index", new { projectId = model.ProjectId });
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        TaskViewModel? task = await _taskService.GetTaskByIdAsync(taskId);
        return PartialView("_UpdateTaskModal", task);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadFile(int taskAttachmentId)
    {
        (FileStream? fileStream, string? fileName) = await _taskService.DownloadFileAsync(taskAttachmentId);

        if (fileStream != null && !string.IsNullOrEmpty(fileName))
        {
            return File(fileStream, "application/octet-stream", fileName);
        }

        return NotFound();
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> UpdateTask(TaskViewModel model)
    {
        if (SessionUser?.UserId == null)
        {
            TempData["ErrorMessage"] = Constants.InvalidSessionMessage;
            return RedirectToAction("Index", new { projectId = model.ProjectId });
        }

        bool isUpdated = await _taskService.UpdateTaskAsync(model, SessionUser.UserId);
        if (isUpdated)
        {
            TempData["SuccessMessage"] = Constants.TaskUpdatedMessage;
            await _hubContext.Clients.All.SendAsync("TaskUpdated");
        }
        else
        {
            TempData["ErrorMessage"] = Constants.TaskUpdateFailedMessage;
        }
        return RedirectToAction("Index", new { projectId = model.ProjectId });
    }

    [CustomAuthorize("Task Board", "CanDelete")]
    [HttpGet]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        int? projectId = await _taskService.DeleteTaskAsync(taskId);
        if (projectId > 0)
        {
            TempData["SuccessMessage"] = Constants.TaskDeletedMessage;
            await _hubContext.Clients.All.SendAsync("TaskDeleted");
        }
        else
        {
            TempData["ErrorMessage"] = Constants.TaskDeleteFailedMessage;
        }
        return RedirectToAction("Index", new { projectId });
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> AddSprint(int projectId)
    {
        ProjectViewModel? project = await _projectService.GetProjectByIdAsync(projectId);

        SprintViewModel sprint = new()
        {
            ProjectId = projectId,
            ProjectStartDate = project?.StartDate ?? DateTime.UtcNow,
            ProjectEndDate = project?.EndDate ?? DateTime.UtcNow,
            ScrumMasterId = project?.AssignedToScrumMaster
        };
        return PartialView("_AddSprintModal", sprint);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    public async Task<IActionResult> CheckSprintNameExists(string name, int projectId = 0)
    {
        await Task.Delay(1000);
        bool exists = await _sprintService.CheckSprintNameExistsAsync(name.Trim(), projectId);
        return Json(!exists);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> AddSprint(SprintViewModel model)
    {
        bool isAdded = await _sprintService.AddSprintAsync(model);
        if(isAdded)
        {
            TempData["SuccessMessage"] = Constants.SprintAddedMessage;
        }
        else
        {
            TempData["ErrorMessage"] = Constants.SprintAddFailedMessage;
        }
        return RedirectToAction("Index", new { model.ProjectId });
    }

    public async Task<IActionResult> GetBacklogTasks(int projectId)
    {
        List<TaskViewModel>? tasks = await _taskService.GetBacklogTasksByProjectIdAsync(projectId);
        return PartialView("_BacklogTasks", tasks);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpGet]
    public async Task<IActionResult> GetSprintById(int sprintId)
    {
        SprintViewModel? sprint = await _sprintService.GetSprintByIdAsync(sprintId);
        return Json(sprint);
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> StartSprint(int sprintId)
    {
        bool isStarted = await _sprintService.StartSprintAsync(sprintId);
        return Json(new { success = isStarted, message = isStarted ? Constants.SprintStartedMessage : Constants.SprintStartFailedMessage });
    }

    [CustomAuthorize("Task Board", "CanAddEdit")]
    [HttpPost]
    public async Task<IActionResult> CompleteSprint(int sprintId)
    {
        bool isCompleted = await _sprintService.CompleteSprintAsync(sprintId);
        return Json(new { success = isCompleted, message = isCompleted ? Constants.SprintCompletedMessage : Constants.SprintCompleteFailedMessage });
    }
}