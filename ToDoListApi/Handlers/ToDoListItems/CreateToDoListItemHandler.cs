using FluentValidation;
using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class CreateToDoListItemRequest : IRequest<CreateToDoListItemResponse>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid>? Tags { get; set; }
}

public class CreateToDoListItemResponse
{
    public Guid Id { get; set; }
    public ToDoListItemStatus ItemStatus { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid> Tags { get; set; }
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
            .WithMessage("Maximum Length Allowed For Description is 250");
        
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