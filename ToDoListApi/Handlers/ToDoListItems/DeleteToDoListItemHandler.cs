using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class DeleteToDoListItemRequest : IRequest<DeleteToDoListItemResponse>
{
    public Guid Id { get; set; }
}

public class DeleteToDoListItemResponse
{
    public bool Success { get; set; }
}

public class DeleteToDoListItemValidator : AbstractValidator<DeleteToDoListItemRequest>
{
    
    public DeleteToDoListItemValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
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
    }
}

public class DeleteToDoListItemHandler : IRequestHandler<DeleteToDoListItemRequest, DeleteToDoListItemResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;

    public DeleteToDoListItemHandler(ToDoListApiDomainContext domainContext)
    {
        _domainContext = domainContext;
    }
    
    public async Task<DeleteToDoListItemResponse> Handle(DeleteToDoListItemRequest request, CancellationToken cancellationToken)
    {
        var tagToDoListItem = await _domainContext.TagToDoListItems.Where(x => x.ToDoListItemId == request.Id)
            .ToListAsync(cancellationToken);

        _domainContext.TagToDoListItems.RemoveRange(tagToDoListItem);
        
        var item = await _domainContext.ToDoListItems
            .Include(x => x.TagToDoListItems)
            .SingleAsync(x => x.Id == request.Id, cancellationToken);
        
        _domainContext.TagToDoListItems.RemoveRange(item.TagToDoListItems);
        _domainContext.ToDoListItems.Remove(item);
        
        await _domainContext.SaveChangesAsync(cancellationToken);
        
        return new DeleteToDoListItemResponse
        {
            Success = true
        };
    }
}