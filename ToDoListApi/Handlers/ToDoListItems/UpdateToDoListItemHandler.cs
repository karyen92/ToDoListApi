using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class UpdateToDoListItemRequest : IRequest<UpdateToDoListItemResponse>
{
    public Guid Id { get; set; }
    public ToDoListItemStatus ItemStatus { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid>? Tags { get; set; }
}

public class UpdateToDoListItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ToDoListItemStatus ItemStatus { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid> Tags { get; set; }
    public DateTime LastUpdateDate { get; set; }
}

public class UpdateToDoListItemValidator : AbstractValidator<UpdateToDoListItemRequest>
{
    
    public UpdateToDoListItemValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        RuleFor(x => x.Id)
            .Custom((id, context) =>
            {
                var item = domainContext.ToDoListItems.SingleOrDefault(x =>
                    x.Id == id && x.CreatedByUserId == identityProvider.UserId);

                if (item == null)
                {
                    context.AddFailure("Invalid Item Id");
                }
            });
        
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


public class UpdateToDoListItemHandler : IRequestHandler<UpdateToDoListItemRequest, UpdateToDoListItemResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;

    public UpdateToDoListItemHandler(ToDoListApiDomainContext domainContext)
    {
        _domainContext = domainContext;
    }

    public async Task<UpdateToDoListItemResponse> Handle(UpdateToDoListItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _domainContext.ToDoListItems
            .Include(x => x.TagToDoListItems)
            .SingleAsync(x => x.Id == request.Id, cancellationToken);

        item.Title = request.Title;
        item.ItemStatus = request.ItemStatus;
        item.Description = request.Description;
        item.Location = request.Location;
        item.DueDate = request.DueDate;
        item.LastUpdateDate = DateTime.UtcNow;

        _domainContext.TagToDoListItems.RemoveRange(item.TagToDoListItems);

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

        return new UpdateToDoListItemResponse
        {
            Id = item.Id,
            Title = item.Title,
            ItemStatus = item.ItemStatus,
            Description = item.Description,
            Location = item.Location,
            DueDate = item.DueDate,
            Tags = tags.Select(x => x.TagId).ToList(),
            LastUpdateDate = item.LastUpdateDate
        };

    }
}