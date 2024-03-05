using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Tags;

public class DeleteTagRequest : IRequest<DeleteTagResponse>
{
    /// <summary>
    /// The tag id
    /// </summary>
    public Guid Id { get; set; }
}

public class DeleteTagResponse
{
    /// <summary>
    /// Indicate the deletion success of the selected tag
    /// </summary>
    public bool Success { get; set; }
}

public class DeleteTagValidator : AbstractValidator<DeleteTagRequest>
{
    public DeleteTagValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        RuleFor(x => x.Id)
            .Custom((id, context) =>
            {
                var tag = domainContext.Tags.SingleOrDefault(x =>
                    x.Id == id && x.CreatedByUserId == identityProvider.UserId);

                if (tag == null)
                {
                    context.AddFailure("Invalid Tag Id");
                }
            });
    }
}

public class DeleteTagHandler : IRequestHandler<DeleteTagRequest, DeleteTagResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;

    public DeleteTagHandler(ToDoListApiDomainContext domainContext)
    {
        _domainContext = domainContext;
    }

    public async Task<DeleteTagResponse> Handle(DeleteTagRequest request, CancellationToken cancellationToken)
    {
        var tagListItem = await _domainContext.TagToDoListItems.Where(x => x.TagId == request.Id)
            .ToListAsync(cancellationToken);

        _domainContext.TagToDoListItems.RemoveRange(tagListItem);
        
        var tag = await _domainContext.Tags.FindAsync(request.Id);
        _domainContext.Tags.Remove(tag!);
        
        await _domainContext.SaveChangesAsync(cancellationToken);

        return new DeleteTagResponse
        {
            Success = true
        };
    }
}