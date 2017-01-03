using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
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

        public JwtBearerOptions Create()
        {
            var jwtBearerOptions = new JwtBearerOptions
            {
                AutomaticAuthenticate = true
            };

            jwtBearerOptions.TokenValidationParameters = _tokenValidationParametersFactory.Create();

            jwtBearerOptions.Events = new JwtBearerEvents()
            {
                //OnTokenValidated = context =>
                //{
                //    return Task.FromResult<object>(null);
                //},
                OnAuthenticationFailed = context =>
                {
                    _logger.LogInformation($"Jwt token validation failed. Exception: {context.Exception.ToString()}");

                    context.Ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(new ClaimsPrincipal(), new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties(), string.Empty);
                    context.HandleResponse();

                    return Task.FromResult<object>(null);
                },
                //OnChallenge = context =>
                //{
                //    return Task.FromResult<object>(null);
                //},
                //OnMessageReceived = context =>
                //{
                //    return Task.FromResult<object>(null);
                //}
            };

            return jwtBearerOptions;
        }
    }
}
