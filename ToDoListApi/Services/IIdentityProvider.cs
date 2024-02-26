using System.Security.Claims;
using ToDoListApi.Extensions;

namespace ToDoListApi.Services;

public interface IIdentityProvider
{
    Guid UserId { get; }
}

public class WebIdentityProvider : IIdentityProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebIdentityProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => _httpContextAccessor.HttpContext.User.GetClaimValue(ClaimTypes.NameIdentifier);
}