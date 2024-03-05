using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Users;

public class GetCurrentUserRequest : IRequest<GetCurrentUserResponse>
{
}

public class GetCurrentUserResponse
{
    /// <summary>
    /// The current user id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The current user username
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The date current user was created in this application
    /// </summary>
    public DateTime CreateDate { get; set; }
}

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, GetCurrentUserResponse>
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ToDoListApiDomainContext _domainContext;

    public GetCurrentUserHandler(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        _domainContext = domainContext;
        _identityProvider = identityProvider;
    }

    public async Task<GetCurrentUserResponse> Handle(GetCurrentUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _domainContext.Users.FindAsync(_identityProvider.UserId);
        return new GetCurrentUserResponse
        {
            Id = user.Id,
            Username = user.Username,
            CreateDate = user.CreateDate
        };
    }
}