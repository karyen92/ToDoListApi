using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Registration;
using ToDoListApi.Services;

namespace ToDoListApiTest.Registration;

[TestClass]
public class RegisterUserHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IAppSettingProvider _appSettingProvider;
    private RegisterUserHandler _handler;
    private RegisterUserRequest _request;
    private RegisterUserResponse _response;
    
    public override void Before()
    {
        _domainContext = Resolve<ToDoListApiDomainContext>();
        _domainContext.Database.EnsureCreated();

        _appSettingProvider = Resolve<IAppSettingProvider>();

        _domainContext.SaveChanges();
        
        _handler = new RegisterUserHandler(_domainContext, _appSettingProvider);
    }
    
    public override void When()
    {
        _request = new RegisterUserRequest
        {
            Username = "user1",
            Password = "abc123"
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Register_Success()
    {
        _response.Id.Should().NotBe(Guid.Empty);
    }
}