using FluentValidation;
using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class CreateToDoListItemRequest : IRequest<CreateToDoListItemResponse>
{
    /// <summary>
    /// The title of the item (required).
    /// Maximum accepted character length is 250
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The description of the item (optional).
    /// Maximum accepted character length is 500
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The location of the item (optional).
    /// Maximum accepted character length is 250
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// The due date of the item (optional).
    /// </summary>
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// List of tags (tag id) that is associated with the to be created items
    /// </summary>
    public List<Guid>? Tags { get; set; }
}

public class CreateToDoListItemResponse
{
    
    /// <summary>
    /// The id of the created item
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The status of the item
    /// By default newly created item is NotStarted
    /// </summary>
    public ToDoListItemStatus ItemStatus { get; set; }
    /// <summary>
    /// The title of the item
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The description of the item
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The location of the item
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// The due date of the item
    /// </summary>
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// The tags associated with the item
    /// </summary>
    public List<Guid> Tags { get; set; }
    /// <summary>
    /// The last update date of the item
    /// For newly created item, the last update date would be equivalent to the create date 
    /// </summary>
    public DateTime LastUpdateDate { get; set; }
}

public class CreateToDoListItemValidator : AbstractValidator<CreateToDoListItemRequest>
{
    
    public CreateToDoListItemValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title Cannot Be Empty")
            .MaximumLength(250)
            .WithMessage("Maximum Length Allowed For Title is 250");
        
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Maximum Length Allowed For Description is 500");
        
        RuleFor(x => x.Location)
            .MaximumLength(250)
            .WithMessage("Maximum Length Allowed For Location is 250");

        RuleForEach(x => x.Tags)
            .SetValidator(new TagExistValidator(domainContext, identityProvider))
            .When(x => x.Tags != null);
    }
}

public class TagExistValidator : AbstractValidator<Guid>
{
    public TagExistValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        RuleFor(x => x).Custom((id, context) =>
        {
            var existingTag =
                domainContext.Tags.SingleOrDefault(x => x.Id == id && x.CreatedByUserId == identityProvider.UserId);

            if (existingTag == null)
            {
                context.AddFailure("Invalid Tag Id");
            }
        });
    }
}

public class CreateToDoListItemHandler : IRequestHandler<CreateToDoListItemRequest, CreateToDoListItemResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IIdentityProvider _identityProvider;

    public CreateToDoListItemHandler(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        _domainContext = domainContext;
        _identityProvider = identityProvider;
    }

    
    public async Task<CreateToDoListItemResponse> Handle(CreateToDoListItemRequest request, CancellationToken cancellationToken)
    {
        var item = new ToDoListItem
        {
            Title = request.Title,
            ItemStatus = ToDoListItemStatus.NotStarted,
            Description = request.Description,
            Location = request.Location,
            DueDate = request.DueDate,
            CreatedByUserId = _identityProvider.UserId,
            LastUpdateDate = DateTime.UtcNow
        };

        _domainContext.ToDoListItems.Add(item);

        var tags = new List<TagToDoListItem>();
        if (request.Tags != null)
        {
            foreach (var tagId in request.Tags)
            {
                var todoItemTag = new TagToDoListItem
                {
                    CreateDate = DateTime.UtcNow,
                    TagId = tagId,
                    ToDoListItemId = item.Id,
                };

                _domainContext.TagToDoListItems.Add(todoItemTag);
                tags.Add(todoItemTag);
            }
        }

        await _domainContext.SaveChangesAsync(cancellationToken);

        return new CreateToDoListItemResponse
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            ItemStatus = item.ItemStatus,
            Location = item.Location,
            DueDate = item.DueDate,
            Tags = tags.Select(x => x.TagId).ToList(),
            LastUpdateDate = item.LastUpdateDate
        };

    }
}