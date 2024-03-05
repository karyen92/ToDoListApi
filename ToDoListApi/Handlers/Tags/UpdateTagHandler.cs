using FluentValidation;
using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Tags;

public class UpdateTagRequest : IRequest<UpdateTagResponse>
{
    /// <summary>
    /// The tag id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The tag new label
    /// </summary>
    public string Label { get; set; }
}

public class UpdateTagResponse
{
    /// <summary>
    /// The updated tag id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The updated tag label
    /// </summary>
    public string Label { get; set; }
}

public class UpdateTagValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
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
        
        RuleFor(x => x.Label)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Label Cannot Be Empty")
            .MaximumLength(30)
            .WithMessage("Maximum Length For Label Is 30")
            .Custom((label, context) =>
            {
                var duplicateLabel = domainContext.Tags.Count(x => x.CreatedByUserId == identityProvider.UserId
                                                                   && x.Label.Trim() == label.Trim()
                                                                   && x.Id != context.InstanceToValidate.Id);

                if (duplicateLabel > 0)
                {
                    context.AddFailure("Duplicated Label");
                }
            });
    }
}

public class UpdateTagHandler : IRequestHandler<UpdateTagRequest, UpdateTagResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;

    public UpdateTagHandler(ToDoListApiDomainContext domainContext)
    {
        _domainContext = domainContext;
    }
    
    public async Task<UpdateTagResponse> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var tag = await _domainContext.Tags.FindAsync(request.Id);
        tag!.Label = request.Label;
        tag.LastUpdateDate = DateTime.UtcNow;

        await _domainContext.SaveChangesAsync(cancellationToken);

        return new UpdateTagResponse
        {
            Id = tag.Id,
            Label = tag.Label
        };
    }
}