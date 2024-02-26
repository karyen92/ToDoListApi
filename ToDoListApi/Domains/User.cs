namespace ToDoListApi.Domains;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public ICollection<ToDoListItem> ToDoListItems { get; set; }
    public ICollection<Tag> Tags { get; set; }
}
