using FakeItEasy;
using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.ToDoListItems;
using ToDoListApi.Services;

namespace ToDoListApiTest.ToDoListItems;

[TestClass]
public class QueryToDoListItemsHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private QueryToDoListItemsHandler _handler;
    private QueryToDoListItemsRequest _request;
    private QueryToDoListItemsResponse _response;

    private Guid _tagId;
    private Guid _item1Id;
    private Guid _item2Id;

    public override void Before()
    {
        _domainContext = Resolve<ToDoListApiDomainContext>();
        _domainContext.Database.EnsureCreated();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            Password = "abc",
            CreateDate = DateTime.UtcNow
        };

        _domainContext.Users.Add(user);

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };

        _tagId = tag.Id;

        _domainContext.Tags.Add(tag);

        var item1 = new ToDoListItem
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = "Description",
            Location = "Location",
            DueDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow,
            CreatedByUserId = user.Id,
            ItemStatus = ToDoListItemStatus.InProgress
        };

        var item2 = new ToDoListItem
        {
            Id = Guid.NewGuid(),
            Title = "Title2",
            Description = "Description2",
            Location = "Location2",
            DueDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow,
            CreatedByUserId = user.Id,
            ItemStatus = ToDoListItemStatus.Completed
        };

        _item1Id = item1.Id;
        _item2Id = item2.Id;

        _domainContext.ToDoListItems.Add(item1);
        _domainContext.ToDoListItems.Add(item2);

        var tagToDoItem = new TagToDoListItem
        {
            Tag = tag,
            ToDoListItem = item1
        };

        _domainContext.TagToDoListItems.Add(tagToDoItem);

        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new QueryToDoListItemsHandler(_domainContext, _identityProvider);
    }

    [TestMethod]
    public void Ensure_Query_By_SearchText_Success()
    {
        _request = new QueryToDoListItemsRequest
        {
            SearchText = "2",
            TakeCount = 10
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
        _response.Total.Should().Be(1);
        var data = _response.Data.First();
        data.Id.Should().Be(_item2Id);

        var dbData = _domainContext.ToDoListItems.Single(x => x.Id == _item2Id);
        data.Title.Should().Be(dbData.Title);
        data.Description.Should().Be(dbData.Description);
        data.Location.Should().Be(dbData.Location);
        data.ItemStatus.Should().Be(dbData.ItemStatus);
        data.DueDate.Should().Be(dbData.DueDate);
        data.LastUpdateDate.Should().Be(dbData.LastUpdateDate);
    }

    [TestMethod]
    public void Ensure_Query_By_Tags_Success()
    {
        _request = new QueryToDoListItemsRequest
        {
            Tags = new List<Guid> { _tagId },
            TakeCount = 10
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
        _response.Total.Should().Be(1);
        var data = _response.Data.First();
        data.Id.Should().Be(_item1Id);
    }
}