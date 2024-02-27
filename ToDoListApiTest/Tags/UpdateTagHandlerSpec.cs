using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Tags;
using ToDoListApi.Services;

namespace ToDoListApiTest.Tags;

[TestClass]
public class UpdateTagHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private UpdateTagHandler _handler;
    private UpdateTagRequest _request;
    private UpdateTagResponse _response;

    private Guid _updateTagId;

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

        _updateTagId = tag.Id;
        
        _domainContext.Tags.Add(tag);

        _handler = new UpdateTagHandler(_domainContext);
    }

    public override void When()
    {
        _request = new UpdateTagRequest
        {
            Id = _updateTagId,
            Label = "Test2"
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Create_Success()
    {
        _response.Label.Should().Be(_request.Label);
        _response.Id.Should().NotBe(Guid.Empty);
    }
}

[TestClass]
public class UpdateTagValidatorSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private UpdateTagValidator _validator;
    private UpdateTagRequest _request;
    
    private Guid _updateTagId;

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
        
        var tag1 = new Tag
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };
        
        var tag2 = new Tag
        {
            Id = Guid.NewGuid(),
            Label = "Test-Fixed",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };

        _updateTagId = tag1.Id;
        
        _domainContext.Tags.Add(tag1);
        _domainContext.Tags.Add(tag2);

        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);
        
        _validator = new UpdateTagValidator(_domainContext, _identityProvider);

    }
    
    [TestMethod]
    public void Ensure_Throw_With_Duplicate_Tag()
    {
        _request = new UpdateTagRequest
        {
            Id = _updateTagId,
            Label = "Test-Fixed"
        };

        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Label).WithErrorMessage("Duplicated Label");
    }
    
}