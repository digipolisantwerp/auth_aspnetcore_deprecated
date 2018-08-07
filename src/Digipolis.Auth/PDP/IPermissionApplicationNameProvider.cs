using System.Security.Claims;

namespace Digipolis.Auth.PDP
{
    public interface IPermissionApplicationNameProvider
    {
        string ApplicationName(ClaimsPrincipal principal);
    }
}