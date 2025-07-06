using Microsoft.EntityFrameworkCore;
using ToDoListManagement.Entity.Models;

namespace ToDoListManagement.Entity.Data;

public partial class ToDoListDbContext : DbContext
{
    public ToDoListDbContext(DbContextOptions<ToDoListDbContext> options)
        : base(options)
    {

    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }
    public DbSet<ToDoList> ToDoLists { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Sprint> Sprint { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 2,
                RoleName = "Program Manager",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 3,
                RoleName = "Member",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 4,
                RoleName = "ScrumMaster",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 1,
                PermissionName = "Projects",
                RoleId = 1,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 2,
                PermissionName = "Employees",
                RoleId = 1,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 3,
                PermissionName = "Task Board",
                RoleId = 1,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 4,
                PermissionName = "Role And Permissions",
                RoleId = 1,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 5,
                PermissionName = "Projects",
                RoleId = 2,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 6,
                PermissionName = "Employees",
                RoleId = 2,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 7,
                PermissionName = "Task Board",
                RoleId = 2,
                CanView = true,
                CanAddEdit = true,
                CanDelete = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 8,
                PermissionName = "Role And Permissions",
                RoleId = 2,
                CanView = false,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 9,
                PermissionName = "Projects",
                RoleId = 3,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 10,
                PermissionName = "Employees",
                RoleId = 3,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 11,
                PermissionName = "Task Board",
                RoleId = 3,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 12,
                PermissionName = "Role And Permissions",
                RoleId = 3,
                CanView = false,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 13,
                PermissionName = "Projects",
                RoleId = 4,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 14,
                PermissionName = "Employees",
                RoleId = 4,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 15,
                PermissionName = "Task Board",
                RoleId = 4,
                CanView = true,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 16,
                PermissionName = "Role And Permissions",
                RoleId = 4,
                CanView = false,
                CanAddEdit = false,
                CanDelete = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User 
            {
                UserId = 1,
                Name = "Admin",
                Email = "admin@outlook.com",
                PasswordHash = "$2a$11$YCiiJxUwumHUtegC05ahFej29UzVm/s1HRwPriPIuta4b.GWddWuW",
                RoleId = 1,
                IsDeleted = false,
                IsActive = true,
                PhoneNumber = "9988556644"
            }
        );

        modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();
    }
}