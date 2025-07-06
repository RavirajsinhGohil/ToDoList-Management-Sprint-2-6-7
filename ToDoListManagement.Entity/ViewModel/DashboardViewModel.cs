namespace ToDoListManagement.Entity.ViewModel;

public class DashboardViewModel
{
    public ProjectViewModel? Project { get; set; }

    public List<ProjectViewModel> Projects { get; set; } = [];

    public List<UserViewModel> ProjectManagers { get; set; } = [];
    
}
