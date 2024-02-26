using Microsoft.EntityFrameworkCore;

namespace ToDoListApi.Domains;

public class ToDoListApiDomainContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ToDoListItem> ToDoListItems { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<TagToDoListItem> TagToDoListItems { get; set; }

    public ToDoListApiDomainContext(DbContextOptions<ToDoListApiDomainContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Username)
                .HasMaxLength(30)
                .IsRequired();
            
            e.Property(x => x.Password)
                .HasMaxLength(64)
                .IsRequired();

            e.Property(x => x.CreateDate)
                .IsRequired();

            e.Property(x => x.LastLoginDate);
        });

        modelBuilder.Entity<ToDoListItem>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Title)
                .HasMaxLength(250)
                .IsRequired();

            e.Property(x => x.Description)
                .HasMaxLength(500);

            e.Property(x => x.ItemStatus)
                .HasConversion<string>()
                .IsRequired();

            e.Property(x => x.Location)
                .HasMaxLength(250);

            e.Property(x => x.LastUpdateDate)
                .IsRequired();

            e.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.ToDoListItems)
                .HasForeignKey(x => x.CreatedByUserId);
        });
        
        modelBuilder.Entity<Tag>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Label)
                .HasMaxLength(30)
                .IsRequired();

            e.Property(x => x.LastUpdateDate)
                .IsRequired();

            e.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.CreatedByUserId);
        });
        
        modelBuilder.Entity<TagToDoListItem>(e =>
        {
            e.HasKey(x => new {x.TagId, x.ToDoListItemId});
            e.HasOne(x => x.Tag)
                .WithMany(x => x.TagToDoListItems)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.NoAction);
            
            e.HasOne(x => x.ToDoListItem)
                .WithMany(x => x.TagToDoListItems)
                .HasForeignKey(x => x.ToDoListItemId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}