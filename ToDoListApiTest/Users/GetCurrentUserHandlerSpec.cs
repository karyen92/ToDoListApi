using FakeItEasy;
using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Users;
using ToDoListApi.Services;

namespace ToDoListApiTest.Users;

[TestClass]
public class GetCurrentUserHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private GetCurrentUserHandler _handler;
    private GetCurrentUserRequest _request;
    private GetCurrentUserResponse _response;

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
        
        _handler = new GetCurrentUserHandler(_domainContext, _identityProvider);
    }

    public override void When()
    {
        _request = new GetCurrentUserRequest();
        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Get_Success()
    {
        _response.Id.Should().Be(_identityProvider.UserId);
        _response.Username.Should().Be("user1");
    }
}