using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDoListApi.Domains;
using ToDoListApi.Extensions;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Login;

public class LoginRequest : IRequest<LoginResponse>
{
    /// <summary>
    /// The username
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The password
    /// </summary>
    public string Password { get; set; }
}

public class LoginResponse
{
    /// <summary>
    /// The generated jwt token
    /// Pass the token at the Headers when calling API
    /// </summary>
    public string Token { get; set; }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator(ToDoListApiDomainContext domainContext, IAppSettingProvider appSettingProvider)
    {
        RuleFor(x => x.Username)
            .Custom((username, context) =>
            {
                if (string.IsNullOrEmpty(username))
                {
                    context.AddFailure("Username Cannot Be Empty");
                    return;
                }
                
                if (string.IsNullOrEmpty(context.InstanceToValidate.Password))
                {
                    context.AddFailure("Password Cannot Be Empty");
                    return;
                }

                var hashedPassword = HashingExtension.Sha256Hash(context.InstanceToValidate.Password,
                    appSettingProvider.PasswordSalt);
                
                var user = domainContext.Users.SingleOrDefault(x => x.Username == username && x.Password == hashedPassword);

                if (user == null)
                {
                    context.AddFailure("Invalid Email Or Password");
                }
            });

    }
}

public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IAppSettingProvider _appSettingProvider;

    public LoginHandler(ToDoListApiDomainContext domainContext, IAppSettingProvider appSettingProvider)
    {
        _domainContext = domainContext;
        _appSettingProvider = appSettingProvider;
    }
    
    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _domainContext.Users.SingleAsync(x => x.Username == request.Username, cancellationToken);
        
        var token = JwtTokenGenerator.GenerateUserToken(_appSettingProvider.JwtKey,_appSettingProvider.JwtIssuer, user);

        return new LoginResponse
        {
            Token = token
        };
    }
}