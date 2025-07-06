using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.Models;
using ToDoListManagement.Entity.ViewModel;

namespace ToDoListManagement.Service.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(int? userId, string? role);
    Task<Pagination<ProjectViewModel>> GetPaginatedProjectsAsync(Pagination<ProjectViewModel> pagination, UserViewModel user);
}
