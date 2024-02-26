using System.Security.Claims;
using System.Security.Principal;

namespace ToDoListApi.Extensions;

public static class ClaimExtension
{
    public static Guid GetClaimValue(this IPrincipal currentPrincipal, string key)
    {
        var identity = currentPrincipal.Identity as ClaimsIdentity;
        var claim = identity?.Claims.First(c => c.Type == key);
        var parseSuccess = Guid.TryParse(claim?.Value, out var userId);
        return parseSuccess ? userId : Guid.Empty;
    }
}