namespace ToDoListApi.Domains;

public enum ToDoListItemStatus { 
    NotStarted = 1,
    InProgress,
    Completed,
    Archived
}

public class ToDoListItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ToDoListItemStatus ItemStatus { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    
    public virtual User CreatedByUser { get; set; }
    public virtual ICollection<TagToDoListItem> TagToDoListItems { get; set; }
}