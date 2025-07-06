using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.Models;

namespace ToDoListManagement.Repository.Interfaces;

public interface IDashboardRepository
{
    Task<Pagination<Project>> GetPaginatedProjectsAsync(Pagination<Project> pagination, int userId, bool isAdmin);
}
