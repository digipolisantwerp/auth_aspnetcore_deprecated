using System.Security.Claims;

namespace Digipolis.Auth.Services
{
    public interface IAuthService
    {
        ClaimsPrincipal User { get; }
    }
}
