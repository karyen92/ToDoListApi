using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.ToDoListItems;
using ToDoListApi.Services;

namespace ToDoListApiTest.ToDoListItems;

[TestClass]
public class CreateToDoListItemHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private CreateToDoListItemHandler _handler;
    private CreateToDoListItemRequest _request;
    private CreateToDoListItemResponse _response;

    private Guid _tagId;

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

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new CreateToDoListItemHandler(_domainContext, _identityProvider);
    }

    public override void When()
    {
        _request = new CreateToDoListItemRequest
        {
            Title = "Title",
            Description = "Description",
            Location = "Location",
            Tags = new List<Guid> { _tagId },
            DueDate = DateTime.UtcNow
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Create_Success()
    {
        _response.Id.Should().NotBe(Guid.Empty);
        _response.ItemStatus.Should().Be(ToDoListItemStatus.NotStarted);
        _response.Title.Should().Be(_request.Title);
        _response.Description.Should().Be(_request.Description);
        _response.Location.Should().Be(_request.Location);
        _response.DueDate.Should().Be(_request.DueDate);
        _response.LastUpdateDate.Should().NotBe(DateTime.MinValue);
        _response.Tags.Should().ContainInOrder(_request.Tags);
    }
}

[TestClass]
public class TagExistSpecValidator : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private TagExistValidator _validator;

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

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };

        _domainContext.Users.Add(user);

        _domainContext.Tags.Add(tag);

        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _validator = new TagExistValidator(_domainContext, _identityProvider);
    }

    [TestMethod]
    public void Ensure_Throw_With_Non_Exist_Tag()
    {
        var result = _validator.TestValidate(Guid.NewGuid());
        result.ShouldHaveValidationErrorFor(x => x).WithErrorMessage("Invalid Tag Id");
    }
}