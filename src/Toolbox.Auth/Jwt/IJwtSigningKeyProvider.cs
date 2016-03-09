using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Toolbox.Auth.Jwt
{
    public interface IJwtSigningKeyProvider
    {
        Task<SecurityKey> ResolveSigningKeyAsync(bool allowCached);
    }
}
