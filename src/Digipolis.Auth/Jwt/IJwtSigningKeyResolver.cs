using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Digipolis.Auth.Jwt
{
    public interface IJwtSigningKeyResolver
    {
        IEnumerable<SecurityKey> IssuerSigningKeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters);
    }
}
