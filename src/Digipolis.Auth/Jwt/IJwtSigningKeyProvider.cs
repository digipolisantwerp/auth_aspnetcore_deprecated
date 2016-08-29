using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Digipolis.Auth.Jwt
{
    public interface IJwtSigningCertificateProvider
    {
        IEnumerable<SecurityKey> IssuerSigningKeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters);
        bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters);
    }
}
