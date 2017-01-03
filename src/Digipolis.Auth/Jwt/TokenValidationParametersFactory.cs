using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

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
            IHostingEnvironment  hostingEnvironment)
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
                ValidateLifetime = true,
                RequireExpirationTime = false,
                NameClaimType = "sub",
                RequireSignedTokens = ShouldRequireSignedTokens(),
                IssuerSigningKeyResolver = _jwtSigningKeyProvider.IssuerSigningKeyResolver,
            };
            
            return tokenValidationParameters;
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
    }
}
