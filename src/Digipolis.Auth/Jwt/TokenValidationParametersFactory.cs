using Digipolis.Auth.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digipolis.Auth.Jwt
{
    public class TokenValidationParametersFactory
    {
        public static TokenValidationParameters Create(AuthOptions authOptions, IJwtSigningKeyResolver jwtSigningKeyProvider)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidAudience = authOptions.JwtAudience,
                ValidateIssuer = false,
                ValidIssuer = authOptions.JwtIssuer,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                NameClaimType = "sub",
                IssuerSigningKeyResolver = jwtSigningKeyProvider.IssuerSigningKeyResolver,
            };
            
            return tokenValidationParameters;
        }
    }
}
