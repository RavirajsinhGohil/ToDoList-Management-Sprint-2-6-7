namespace ToDoListManagement.Entity.Helper;

public class Pagination<T>
{
    public List<T> Items { get; set; } = new List<T>();

    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int TotalRecords { get; set; }
    public int FromRec { get; set; }
    public int ToRec { get; set; }
    public int PageSize { get; set; }
    public string? SearchKeyword { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } 

}
