using System.Threading.Tasks;

namespace Digipolis.Auth.Jwt
{
    public interface ITokenRefreshAgent
    {
        Task<string> RefreshTokenAsync(string token);
        Task<string> LogoutTokenAsync(string userName, string redirectUrl);
    }
}