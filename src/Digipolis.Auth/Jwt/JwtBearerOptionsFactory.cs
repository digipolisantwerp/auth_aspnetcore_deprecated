using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

namespace Digipolis.Auth.Jwt
{
    internal class JwtBearerOptionsFactory
    {
        private readonly ILogger<JwtBearerOptionsFactory> _logger;
        private readonly ITokenValidationParametersFactory _tokenValidationParametersFactory;

        public JwtBearerOptionsFactory(ITokenValidationParametersFactory tokenValidationParametersFactory,
            ILogger<JwtBearerOptionsFactory> logger)
        {
            if (tokenValidationParametersFactory == null) throw new ArgumentNullException(nameof(tokenValidationParametersFactory), $"{nameof(tokenValidationParametersFactory)} cannot be null");
            if (logger == null) throw new ArgumentNullException(nameof(logger), $"{nameof(logger)} cannot be null");

            _tokenValidationParametersFactory = tokenValidationParametersFactory;
            _logger = logger;
        }

        public void Setup(JwtBearerOptions jwtBearerOptions)
        {
            jwtBearerOptions.TokenValidationParameters = _tokenValidationParametersFactory.Create();

            jwtBearerOptions.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    if (context.HttpContext.Request.Path.Value.StartsWith("/signalr") && context.HttpContext.Request.Path.Value.EndsWith("negotiate") &&
                        context.Request.Query.TryGetValue("access_token", out var accessToken) && !string.IsNullOrWhiteSpace(accessToken.FirstOrDefault()))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ValidateToken(accessToken.First().Replace("Bearer ", string.Empty), jwtBearerOptions.TokenValidationParameters, out var validatedToken);

                        context.HttpContext.User.AddIdentity((ClaimsIdentity)token.Identity);
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    _logger.LogInformation($"Jwt token validation failed. Exception: {context.Exception.ToString()}");

                    return Task.FromResult<object>(null);
                },
            };
        }
    }
}
