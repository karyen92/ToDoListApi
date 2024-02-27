using FakeItEasy;
using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.ToDoListItems;
using ToDoListApi.Services;

namespace ToDoListApiTest.ToDoListItems;

[TestClass]
public class DeleteToDoListItemHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private DeleteToDoListItemHandler _handler;
    private DeleteToDoListItemRequest _request;
    private DeleteToDoListItemResponse _response;

    private Guid _itemId;

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
        
        _domainContext.Tags.Add(tag);

        var item = new ToDoListItem
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

        _itemId = item.Id;

        _domainContext.ToDoListItems.Add(item);

        var tagToDoItem = new TagToDoListItem
        {
            Tag = tag,
            ToDoListItem = item
        };

        _domainContext.TagToDoListItems.Add(tagToDoItem);
        
        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new DeleteToDoListItemHandler(_domainContext);
    }

    public override void When()
    {
        _request = new DeleteToDoListItemRequest
        {
            Id = _itemId
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Delete_Success()
    {
        _response.Success.Should().BeTrue();
        var item = _domainContext.ToDoListItems.Find(_itemId);
        var tagItem = _domainContext.TagToDoListItems.SingleOrDefault(x => x.ToDoListItemId == _itemId);
        item.Should().BeNull();
        tagItem.Should().BeNull();
    }
}