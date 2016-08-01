using System.Threading.Tasks;

namespace Digipolis.Auth.Jwt
{
    public interface ITokenRefreshHandler
    {
        Task<string> HandleRefreshAsync(string token);
    }
}