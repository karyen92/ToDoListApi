using FakeItEasy;
using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.ToDoListItems;
using ToDoListApi.Services;

namespace ToDoListApiTest.ToDoListItems;

[TestClass]
public class UpdateToDoListItemHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private UpdateToDoListItemHandler _handler;
    private UpdateToDoListItemRequest _request;
    private UpdateToDoListItemResponse _response;

    private Guid _tagId;
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

        _tagId = tag.Id;

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
        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new UpdateToDoListItemHandler(_domainContext);
    }

    public override void When()
    {
        _request = new UpdateToDoListItemRequest
        {
            Id = _itemId,
            Title = "Title",
            Description = "Description",
            Location = "Location",
            Tags = new List<Guid> { _tagId },
            DueDate = DateTime.UtcNow
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Update_Success()
    {
        _response.Id.Should().NotBe(Guid.Empty);
        _response.Title.Should().Be(_request.Title);
        _response.Description.Should().Be(_request.Description);
        _response.Location.Should().Be(_request.Location);
        _response.DueDate.Should().Be(_request.DueDate);
        _response.LastUpdateDate.Should().NotBe(DateTime.MinValue);
        _response.Tags.Should().ContainInOrder(_request.Tags);
    }
}