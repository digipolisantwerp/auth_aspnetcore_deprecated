using System.Threading.Tasks;

namespace Toolbox.Auth.Jwt
{
    public interface ITokenRefreshHandler
    {
        Task<string> HandleRefreshAsync(string token);
    }
}