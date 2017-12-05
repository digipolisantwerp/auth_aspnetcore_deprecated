using Digipolis.Auth.Options;
using System.Threading.Tasks;

namespace Digipolis.Auth.Jwt
{
    public interface ITokenRefreshAgent
    {
        AuthOptions AuthOptions { get; }
        Task<string> RefreshTokenAsync(string token);
        Task<string> LogoutTokenAsync(string userName, string redirectUrl);
    }
}