using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
                OnAuthenticationFailed = context =>
                {
                    _logger.LogInformation($"Jwt token validation failed. Exception: {context.Exception.ToString()}");

                    return Task.FromResult<object>(null);
                },
            };
        }
    }
}
