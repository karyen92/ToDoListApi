using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Tags;
using ToDoListApi.Services;

namespace ToDoListApiTest.Tags;

[TestClass]
public class CreateTagHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private CreateTagHandler _handler;
    private CreateTagRequest _request;
    private CreateTagResponse _response;

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
        
        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new CreateTagHandler(_domainContext, _identityProvider);
    }

    public override void When()
    {
        _request = new CreateTagRequest
        {
            Label = "Test"
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Create_Success()
    {
        _response.Label.Should().Be(_request.Label);
        _response.Id.Should().NotBe(Guid.Empty);
        _response.LastUpdateDate.Should().NotBe(DateTime.MinValue);
    }
}

[TestClass]
public class CreateTagValidatorSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private CreateTagValidator _validator;
    private CreateTagRequest _request;

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
            Label = "Test",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };
        
        _domainContext.Users.Add(user);

        _domainContext.Tags.Add(tag);

        _domainContext.SaveChanges();
        
        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);
        
        _validator = new CreateTagValidator(_domainContext, _identityProvider);
    }
    
    [TestMethod]
    public void Ensure_Throw_With_Duplicate_Tag()
    {
        _request = new CreateTagRequest
        {
            Label = "Test"
        };

        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Label).WithErrorMessage("Duplicated Label");
    }
}