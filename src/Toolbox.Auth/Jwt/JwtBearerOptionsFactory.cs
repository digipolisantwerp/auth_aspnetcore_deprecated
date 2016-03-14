using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    internal class JwtBearerOptionsFactory
    {
        public static JwtBearerOptions Create(AuthOptions authOptions, IJwtSigningKeyProvider signingKeyProvider, IJwtTokenSignatureValidator signatureValidator,
            ILogger<JwtBearerMiddleware> logger)
        {
            var jwtBearerOptions = new JwtBearerOptions
            {
                AutomaticAuthenticate = true
            };

            jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
            jwtBearerOptions.TokenValidationParameters.ValidAudience = authOptions.JwtAudience;
            jwtBearerOptions.TokenValidationParameters.ValidateIssuer = true;
            jwtBearerOptions.TokenValidationParameters.ValidIssuer = authOptions.JwtIssuer;
            jwtBearerOptions.TokenValidationParameters.ValidateLifetime = true;
            jwtBearerOptions.TokenValidationParameters.ValidateSignature = true;
            jwtBearerOptions.TokenValidationParameters.SignatureValidator = signatureValidator.SignatureValidator;
            jwtBearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(authOptions.JwtValidatorClockSkew);
            jwtBearerOptions.TokenValidationParameters.NameClaimType = "sub";

            jwtBearerOptions.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    //ToDo what and how to log?
                    logger.LogInformation(context.Exception.ToString());

                    context.AuthenticationTicket = new Microsoft.AspNet.Authentication.AuthenticationTicket(new ClaimsPrincipal(), new Microsoft.AspNet.Http.Authentication.AuthenticationProperties(), string.Empty);
                    context.HandleResponse();

                    return Task.FromResult<object>(null);
                },
                OnChallenge = context =>
                {
                    return Task.FromResult<object>(null);
                },
                OnReceivedToken = async context =>
                {
                    //the signingKey is resolved on this event because we can make the call async here, in the signatureValidator async is not possible
                    context.Options.TokenValidationParameters.IssuerSigningKey = await signingKeyProvider.ResolveSigningKeyAsync(true);
                },
                OnValidatedToken = context =>
                {
                    return Task.FromResult<object>(null);
                },
                OnReceivingToken = context =>
                {
                    return Task.FromResult<object>(null);
                }
            };

            return jwtBearerOptions;
        }
    }
}
