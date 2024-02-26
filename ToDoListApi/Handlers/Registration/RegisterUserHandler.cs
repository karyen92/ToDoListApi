using FluentValidation;
using MediatR;
using ToDoListApi.Domains;
using ToDoListApi.Extensions;
using ToDoListApi.Services;

namespace ToDoListApi.Handlers.Registration;

public class RegisterUserRequest : IRequest<RegisterUserResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterUserResponse
{
    public Guid Id { get; set; }
}

public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator(ToDoListApiDomainContext domainContext)
    {
        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email Cannot Be Empty")
            .MaximumLength(30)
            .WithMessage("Maximum Length For Username is 30")
            .Custom((email, context) =>
            {
                var existingEmail = domainContext.Users.SingleOrDefault(x => x.Username == email);
                if (existingEmail != null)
                {
                    context.AddFailure("Email Already Used");
                }
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password Cannot Be Empty")
            .MinimumLength(6)
            .WithMessage("Minimum Length For Password Is 6");
    }
}

public class RegisterUserHandler : IRequestHandler<RegisterUserRequest, RegisterUserResponse>
{
    private readonly ToDoListApiDomainContext _domainContext;
    private readonly IAppSettingProvider _appSettingProvider;
    
    public RegisterUserHandler(ToDoListApiDomainContext domainContext, IAppSettingProvider appSettingProvider)
    {
        _domainContext = domainContext;
        _appSettingProvider = appSettingProvider;
    }
    
    public async Task<RegisterUserResponse> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = request.Username,
            Password = HashingExtension.Sha256Hash(request.Password, _appSettingProvider.PasswordSalt),
            CreateDate = DateTime.UtcNow
        };

        _domainContext.Users.Add(user);
        await _domainContext.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse
        {   
            Id = user.Id
        };
    }
}