using FakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Tags;
using ToDoListApi.Services;

namespace ToDoListApiTest.Tags;

[TestClass]
public class DeleteTagHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private DeleteTagHandler _handler;
    private DeleteTagRequest _request;
    private DeleteTagResponse _response;

    private Guid _deleteTagId;

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

        _deleteTagId = tag.Id;
        
        _domainContext.Tags.Add(tag);

        _handler = new DeleteTagHandler(_domainContext);
    }

    public override void When()
    {
        _request = new DeleteTagRequest
        {
            Id = _deleteTagId,
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Create_Success()
    {
        _response.Success.Should().BeTrue();

        var tag = _domainContext.Tags.Find(_deleteTagId);
        tag.Should().BeNull();
    }
}

[TestClass]
public class DeleteTagValidatorSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private DeleteTagValidator _validator;
    private DeleteTagRequest _request;
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

        _domainContext.SaveChanges();

        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);
        
        _validator = new DeleteTagValidator(_domainContext, _identityProvider);

    }
    
    [TestMethod]
    public void Ensure_Throw_With_Invalid_Id()
    {
        _request = new DeleteTagRequest
        {
            Id = Guid.NewGuid()
        };

        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("Invalid Tag Id");
    }
    
}