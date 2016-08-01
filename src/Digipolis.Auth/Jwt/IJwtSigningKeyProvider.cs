using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Digipolis.Auth.Jwt
{
    public interface IJwtSigningKeyProvider
    {
        Task<SecurityKey> ResolveSigningKeyAsync(bool allowCached);
    }
}
