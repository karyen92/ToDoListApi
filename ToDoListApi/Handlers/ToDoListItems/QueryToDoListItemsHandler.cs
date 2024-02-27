using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class QueryToDoListItemsRequest : IRequest<QueryToDoListItemsResponse>
{
    public string? SearchText { get; set; }
    public string? Location { get; set; }
    public List<Guid>? Tags { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DueDate { get; set; }
    public ToDoListItemStatus? ItemStatus { get; set; }
    public int SkipCount { get; set; }
    public int TakeCount { get; set; }
}

public class QueryToDoListItemsResponse
{
    public int Total { get; set; }
    public List<QueryToDoListItemDto> Data { get; set; }
}

public class QueryToDoListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ToDoListItemStatus ItemStatus { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public List<Guid> TagIds { get; set; }
}

public class QueryToDoListItemsHandler : IRequestHandler<QueryToDoListItemsRequest, QueryToDoListItemsResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IIdentityProvider _identityProvider;

    public QueryToDoListItemsHandler(ToDoListApiDomainContext domainContext, IIdentityProvider identityProvider)
    {
        _domainContext = domainContext;
        _identityProvider = identityProvider;
    }
    
    public async Task<QueryToDoListItemsResponse> Handle(QueryToDoListItemsRequest request, CancellationToken cancellationToken)
    {
        var query = _domainContext.ToDoListItems
            .Include(x => x.TagToDoListItems)
            .Where(x => x.CreatedByUserId == _identityProvider.UserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(x => x.Title.Contains(request.SearchText) 
                                     || (x.Description != null && x.Description.Contains(request.SearchText)));
        }
        
        if (!string.IsNullOrEmpty(request.Location))
        {
            query = query.Where(x => x.Location != null && x.Location.Contains(request.Location));
        }
        
        if (request.ItemStatus != null)
        {
            query = query.Where(x => x.ItemStatus == request.ItemStatus);
        }

        if (request.StartDate != null)
        {
            query = query.Where(x => x.LastUpdateDate >= request.StartDate);
        }

        if (request.EndDate != null)
        {
            query = query.Where(x => x.LastUpdateDate <= request.StartDate);
        }
        
        if (request.DueDate != null)
        {
            query = query.Where(x => x.DueDate == request.DueDate);
        }

        if (request.Tags != null)
        {
            foreach (var tagId in request.Tags)
            {

                query = query.Where(x => x.TagToDoListItems.Any(x => x.TagId == tagId));
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderByDescending(x => x.LastUpdateDate)
            .Skip(request.SkipCount)
            .Take(request.TakeCount)
            .Select(x => new QueryToDoListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                ItemStatus = x.ItemStatus,
                Description = x.Description,
                Location = x.Location,
                LastUpdateDate = x.LastUpdateDate,
                DueDate = x.DueDate,
                TagIds = x.TagToDoListItems.Select(y => y.TagId).ToList()
            })
            .ToListAsync(cancellationToken);
        
        return new QueryToDoListItemsResponse
        {
            Total = total,
            Data = data
        };
    }
}