using FluentValidation;
using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Tags;

public class CreateTagRequest : IRequest<CreateTagResponse>
{
    /// <summary>
    /// The label of the tag
    /// Label must be distinct. No duplicate label is allowed per user
    /// </summary>
    public string Label { get; set; }
}

public class CreateTagResponse
{
    /// <summary>
    /// The id of the tag
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The label of the tag
    /// </summary>
    public string Label { get; set; }
    /// <summary>
    /// The last updated date
    /// </summary>
    public DateTime LastUpdateDate { get; set; }
}

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        RuleFor(x => x.Label)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Label Cannot Be Empty")
            .MaximumLength(30)
            .WithMessage("Maximum Length For Label Is 30")
            .Custom((label, context) =>
            {
                var duplicateLabel = domainContext.Tags.Count(x => x.CreatedByUserId == identityProvider.UserId
                                                                   && x.Label.Trim() == label.Trim());

                if (duplicateLabel > 0)
                {
                    context.AddFailure("Duplicated Label");
                }
            });
    }
}

public class CreateTagHandler : IRequestHandler<CreateTagRequest, CreateTagResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IIdentityProvider _identityProvider;

    public CreateTagHandler(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        _domainContext = domainContext;
        _identityProvider = identityProvider;
    }
    
    public async Task<CreateTagResponse> Handle(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var tag = new Tag
        {
            Label = request.Label.Trim(),
            CreatedByUserId = _identityProvider.UserId,
            LastUpdateDate = DateTime.UtcNow
        };

        _domainContext.Tags.Add(tag);
        await _domainContext.SaveChangesAsync(cancellationToken);

        return new CreateTagResponse
        {
            Id = tag.Id,
            Label = tag.Label,
            LastUpdateDate = tag.LastUpdateDate
        };
    }
}