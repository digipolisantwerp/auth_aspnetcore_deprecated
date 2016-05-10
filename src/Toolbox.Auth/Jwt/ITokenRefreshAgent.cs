using System.Threading.Tasks;

namespace Toolbox.Auth.Jwt
{
    public interface ITokenRefreshAgent
    {
        Task<string> RefreshTokenAsync(string token);
    }
}