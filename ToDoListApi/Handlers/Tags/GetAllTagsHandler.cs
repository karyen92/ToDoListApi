using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Tags;

public class GetAllTagsRequest : IRequest<GetAllTagsResponse>
{
    
}

public class GetAllTagsResponse
{
    public List<GetAllTagsItem> Data { get; set; }
}

public class GetAllTagsItem
{
    public Guid Id { get; set; }
    public string Label { get; set; }
    public DateTime LastUpdateDate { get; set; }
}

public class GetAllTagsHandler : IRequestHandler<GetAllTagsRequest, GetAllTagsResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IIdentityProvider _identityProvider;

    public GetAllTagsHandler(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        _domainContext = domainContext;
        _identityProvider = identityProvider;
    }
    
    public async Task<GetAllTagsResponse> Handle(GetAllTagsRequest request, CancellationToken cancellationToken)
    {
        var data = await _domainContext.Tags
            .Where(x => x.CreatedByUserId == _identityProvider.UserId)
            .Select(x => new GetAllTagsItem
            {
                Id = x.Id,
                Label = x.Label,
                LastUpdateDate = x.LastUpdateDate
            })
            .ToListAsync(cancellationToken);

        return new GetAllTagsResponse
        {
            Data = data
        };
    }
}