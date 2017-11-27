using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Digipolis.Auth.Jwt
{
    public class TokenValidationParametersFactory : ITokenValidationParametersFactory
    {
        private readonly AuthOptions _authOptions;
        private readonly DevPermissionsOptions _devPermissionsOptions;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IJwtSigningKeyResolver _jwtSigningKeyProvider;

        public TokenValidationParametersFactory(IOptions<AuthOptions> authOptions,
            IJwtSigningKeyResolver jwtSigningKeyProvider,
            IOptions<DevPermissionsOptions> devPermissionsOptions,
            IHostingEnvironment hostingEnvironment)
        {
            if (authOptions == null) throw new ArgumentNullException(nameof(authOptions), $"{nameof(authOptions)} cannot be null");
            if (jwtSigningKeyProvider == null) throw new ArgumentNullException(nameof(jwtSigningKeyProvider), $"{nameof(jwtSigningKeyProvider)} cannot be null");
            if (devPermissionsOptions == null) throw new ArgumentNullException(nameof(devPermissionsOptions), $"{nameof(devPermissionsOptions)} cannot be null");
            if (hostingEnvironment == null) throw new ArgumentNullException(nameof(hostingEnvironment), $"{nameof(hostingEnvironment)} cannot be null");

            _authOptions = authOptions.Value;
            _jwtSigningKeyProvider = jwtSigningKeyProvider;
            _devPermissionsOptions = devPermissionsOptions.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public TokenValidationParameters Create()
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidAudience = _authOptions.JwtAudience,
                ValidateIssuer = false,
                ValidIssuer = _authOptions.JwtIssuer,
                ValidateLifetime = ShouldValidateLifetime(),
                RequireExpirationTime = false,
                RequireSignedTokens = ShouldRequireSignedTokens(),
                IssuerSigningKeyResolver = _jwtSigningKeyProvider.IssuerSigningKeyResolver,
                NameClaimTypeRetriever = NameClaimTypeRetriever
            };

            return tokenValidationParameters;
        }

        internal string NameClaimTypeRetriever(SecurityToken token, string y)
        {
            if (_authOptions.EnableServiceAccountAuthorization && token is JwtSecurityToken)
            {
                var jwtToken = (JwtSecurityToken)token;
                var subClaim = jwtToken.Claims?.FirstOrDefault(x => x.Type == Claims.Sub)?.Value;
                if (String.IsNullOrWhiteSpace(subClaim))
                {
                    return Claims.XConsumerUsername;
                }
            }

            return Claims.Sub;
        }

        private bool ShouldRequireSignedTokens()
        {
            var requireSignedTokens = true;

            if (_hostingEnvironment.IsEnvironment(_devPermissionsOptions.Environment) && _devPermissionsOptions.RequireSignedTokens == false)
            {
                requireSignedTokens = false;
            }

            return requireSignedTokens;
        }

        private bool ShouldValidateLifetime()
        {
            var validateLifetime = true;

            if (_hostingEnvironment.IsEnvironment(_devPermissionsOptions.Environment) && _devPermissionsOptions.ValidateTokenLifetime == false)
            {
                validateLifetime = false;
            }

            return validateLifetime;
        }
    }
}
