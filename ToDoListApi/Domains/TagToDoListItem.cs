namespace ToDoListApi.Domains;

public class TagToDoListItem
{
    public Guid ToDoListItemId { get; set; }
    public Guid TagId { get; set; }
    public DateTime CreateDate { get; set; }
    public virtual ToDoListItem ToDoListItem { get; set; }
    public virtual Tag Tag { get; set; }
}