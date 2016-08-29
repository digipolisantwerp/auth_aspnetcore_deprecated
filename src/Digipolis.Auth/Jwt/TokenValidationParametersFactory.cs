using Microsoft.IdentityModel.Tokens;
using System;
using Digipolis.Auth.Options;
using System.Text;

namespace Digipolis.Auth.Jwt
{
    public class TokenValidationParametersFactory
    {
        public static TokenValidationParameters Create(AuthOptions authOptions, IJwtSigningCertificateProvider jwtSigningKeyProvider)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidAudience = authOptions.JwtAudience,
                ValidateIssuer = false,
                ValidIssuer = authOptions.JwtIssuer,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                //LifetimeValidator = jwtSigningKeyProvider.LifetimeValidator,
                NameClaimType = "sub",
                IssuerSigningKeyResolver = jwtSigningKeyProvider.IssuerSigningKeyResolver,
            };
            
            return tokenValidationParameters;
        }
    }
}
