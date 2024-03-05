using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.ToDoListItems;

public class QueryToDoListItemsRequest : IRequest<QueryToDoListItemsResponse>
{
    /// <summary>
    /// Query items based on fields Title and Description
    /// </summary>
    public string? SearchText { get; set; }
    /// <summary>
    /// Query items by Location
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// Query items by a list of tags associated
    /// </summary>
    public List<Guid>? Tags { get; set; }
    /// <summary>
    /// Query the item by LastUpdateDate
    /// Will return item updated after selected date
    /// </summary>
    public DateTime? StartDate { get; set; }
    /// <summary>
    /// Query the item by LastUpdateDate
    /// Will return item updated before selected date
    /// </summary>
    public DateTime? EndDate { get; set; }
    /// <summary>
    /// Query the item by DueDate
    /// </summary>
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// Query the item by ItemStatus
    /// Accepted Values: NotStarted, InProgress, Completed, Archived
    /// </summary>
    public ToDoListItemStatus? ItemStatus { get; set; }
    /// <summary>
    /// Sort the item by selected fields
    /// Accepted values: dueDate, title and lastUpdate
    /// </summary>
    public string? OrderBy { get; set; }
    /// <summary>
    /// Sort the item in descending order by values passed in OrderBy
    /// If left null or set to true, the items are sorted in ascending mode
    /// </summary>
    public bool? IsDescending { get; set; }
    /// <summary>
    /// Use for pagination Pagination: Skip the given number of items
    /// </summary>
    public int SkipCount { get; set; }
    /// <summary>
    /// Use for pagination Pagination: Return the given number of items
    /// </summary>
    public int TakeCount { get; set; }
}

public class QueryToDoListItemsResponse
{
    /// <summary>
    /// Total Number of items filtered
    /// </summary>
    public int Total { get; set; }
    /// <summary>
    /// List of returned items
    /// </summary>
    public List<QueryToDoListItemDto> Data { get; set; }
}

public class QueryToDoListItemDto
{
    /// <summary>
    /// The item id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The item title
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The item status
    /// </summary>
    public ToDoListItemStatus ItemStatus { get; set; }
    /// <summary>
    /// The item description
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The item location
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// The item due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// The last updated date of item
    /// </summary>
    public DateTime LastUpdateDate { get; set; }
    /// <summary>
    /// The tags (id) associated with this item
    /// </summary>
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

        if (request.OrderBy != null)
        {
            if (request.IsDescending is true)
            {
                switch (request.OrderBy)
                {
                    case "dueDate":
                        query = query.OrderByDescending(x => x.DueDate);
                        break;
                    case "title":
                        query = query.OrderByDescending(x => x.Title);
                        break;
                    case "lastUpdate":
                        query = query.OrderByDescending(x => x.LastUpdateDate);
                        break;
                }
            }
            else
            {
                switch (request.OrderBy)
                {
                    case "dueDate":
                        query = query.OrderBy(x => x.DueDate);
                        break;
                    case "title":
                        query = query.OrderByDescending(x => x.Title);
                        break;
                    case "lastUpdate":
                        query = query.OrderBy(x => x.LastUpdateDate);
                        break;
                }
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