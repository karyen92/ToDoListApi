using FluentAssertions;
using FluentValidation.TestHelper;
using ToDoListApi.Domains;
using ToDoListApi.Extensions;
using ToDoListApi.Handlers.Login;
using ToDoListApi.Services;

namespace ToDoListApiTest.Login;

[TestClass]
public class LoginHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IAppSettingProvider _appSettingProvider;
    private LoginHandler _handler;
    private LoginRequest _request;
    private LoginResponse _response;
    
    public override void Before()
    {
        _domainContext = Resolve<ToDoListApiDomainContext>();
        _domainContext.Database.EnsureCreated();

        _appSettingProvider = Resolve<IAppSettingProvider>();

        _domainContext.Users.Add(new User
        {
            Username = "user1",
            Password = HashingExtension.Sha256Hash("pass", _appSettingProvider.PasswordSalt),
            CreateDate = DateTime.UtcNow
        });

        _domainContext.SaveChanges();
        
        _handler = new LoginHandler(_domainContext, _appSettingProvider);
    }

    public override void When()
    {
        _request = new LoginRequest
        {
            Username = "user1",
            Password = "abc123"
        };

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Login_Success()
    {
        _response.Token.Should().NotBeNull();
    }
}

[TestClass]
public class LoginValidatorSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IAppSettingProvider _appSettingProvider;
    private LoginValidator _validator;
    private LoginRequest _request;

    public override void Before()
    {
        _domainContext = Resolve<ToDoListApiDomainContext>();
        _domainContext.Database.EnsureCreated();

        _appSettingProvider = Resolve<IAppSettingProvider>();

        _domainContext.Users.Add(new User
        {
            Username = "user1",
            Password = HashingExtension.Sha256Hash("pass", _appSettingProvider.PasswordSalt),
            CreateDate = DateTime.UtcNow
        });

        _domainContext.SaveChanges();
        
        _validator = new LoginValidator(_domainContext, _appSettingProvider);
    }

    [TestMethod]
    public void Ensure_Throw_With_Invalid_Username_Or_Password()
    {
        _request = new LoginRequest
        {
            Username = "x",
            Password = "u"
        };

        var result = _validator.TestValidate(_request);
        result.ShouldHaveValidationErrorFor(x => x.Username).WithErrorMessage("Invalid Email Or Password");
    }
}