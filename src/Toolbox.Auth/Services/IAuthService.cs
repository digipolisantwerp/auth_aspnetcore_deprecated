using System.Security.Claims;

namespace Toolbox.Auth.Services
{
    public interface IAuthService
    {
        ClaimsPrincipal User { get; }
    }
}
