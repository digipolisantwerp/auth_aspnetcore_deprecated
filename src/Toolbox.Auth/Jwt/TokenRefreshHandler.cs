using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    public class TokenRefreshHandler : ITokenRefreshHandler
    {
        private AuthOptions _authOptions;
        private readonly ILogger<TokenRefreshHandler> _logger;
        private readonly ITokenRefreshAgent _tokenRefreshAgent;

        public TokenRefreshHandler(IOptions<AuthOptions> options, ITokenRefreshAgent tokenRefreshAgent, ILogger<TokenRefreshHandler> logger)
        {
            _authOptions = options.Value;
            _tokenRefreshAgent = tokenRefreshAgent;
            _logger = logger;
        }

        public Task<string> HandleRefreshAsync(string token)
        {
            JwtSecurityToken jwt = null;

            try
            {
                jwt = new JwtSecurityToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Invalid jwt refresh request. token: {token}", ex);
                return Task.FromResult<string>(null);
            }

            if (jwt.ValidTo < DateTime.UtcNow.AddMinutes(_authOptions.TokenRefreshTime))
            {
                if (jwt.Audiences.FirstOrDefault() == _authOptions.JwtAudience)
                {
                    _logger.LogDebug($"Jwt refreshed, token: {token}");
                    return _tokenRefreshAgent.RefreshTokenAsync(token);
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}
