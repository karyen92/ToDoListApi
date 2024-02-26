namespace ToDoListApi.Domains;

public class Tag
{
    public Guid Id { get; set; }
    public string Label { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public virtual User CreatedByUser { get; set; }
    
    public virtual ICollection<TagToDoListItem> TagToDoListItems { get; set; }
}