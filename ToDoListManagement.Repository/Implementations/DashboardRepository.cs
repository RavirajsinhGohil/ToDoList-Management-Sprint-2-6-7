using Microsoft.EntityFrameworkCore;
using ToDoListManagement.Entity.Data;
using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.Models;
using ToDoListManagement.Repository.Interfaces;

namespace ToDoListManagement.Repository.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly ToDoListDbContext _context;
    public DashboardRepository(ToDoListDbContext context)
    {
        _context = context;
    }

    public async Task<Pagination<Project>> GetPaginatedProjectsAsync(Pagination<Project> pagination, int userId, bool isAdmin)
    {
        IQueryable<Project> query = _context.Projects
            .Where(p => !p.IsDeleted)
            .Include(pu => pu.ProjectUsers)
            .ThenInclude(u => u.User)
            .Include(p => p.AssignedPM);

        if (!isAdmin)
        {
            query = query.Where(pu => pu.ProjectUsers != null && pu.ProjectUsers.Any(u => u.UserId == userId && u.User != null && !u.User.IsDeleted));
        }

        if (!string.IsNullOrEmpty(pagination.SearchKeyword))
        {
            query = query.Where(p => p.ProjectName != null && p.ProjectName.ToLower().Contains(pagination.SearchKeyword.Trim().ToLower()));
        }

        int totalRecords = await query.CountAsync();

        query = pagination.SortColumn switch
        {
            "projectName" => pagination.SortDirection?.ToLower() == "asc"
                                ? query.OrderBy(p => p.ProjectName)
                                : query.OrderByDescending(p => p.ProjectName),
            _ => pagination.SortDirection?.ToLower() == "asc"
                                ? query.OrderBy(p => p.ProjectId)
                                : query.OrderByDescending(p => p.ProjectId),
        };

        List<Project> pagedData = await query
            .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        int totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        return new Pagination<Project>
        {
            Items = pagedData,
            CurrentPage = pagination.CurrentPage,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }
}