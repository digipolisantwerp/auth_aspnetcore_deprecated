using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Http;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    internal class JwtBearerOptionsFactory
    {
        public static JwtBearerOptions Create(AuthOptions authOptions, IJwtSigningKeyProvider signingKeyProvider, IJwtTokenSignatureValidator signatureValidator)
        {
            var jwtBearerOptions = new JwtBearerOptions
            {
                AutomaticAuthenticate = true
            };

            jwtBearerOptions.TokenValidationParameters.ValidateActor = false;

            jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
            jwtBearerOptions.TokenValidationParameters.ValidAudience = authOptions.JwtAudience;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuer = false;
            jwtBearerOptions.TokenValidationParameters.ValidIssuer = authOptions.JwtIssuer;

            jwtBearerOptions.TokenValidationParameters.ValidateLifetime = true;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey = false;
            jwtBearerOptions.TokenValidationParameters.ValidateSignature = true;
            jwtBearerOptions.TokenValidationParameters.SignatureValidator = signatureValidator.SignatureValidator;

            jwtBearerOptions.TokenValidationParameters.ClockSkew = new System.TimeSpan(0, 1, 0);

            jwtBearerOptions.TokenValidationParameters.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"; // authOptions.JwtUserIdClaimType;

            jwtBearerOptions.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    //Return 401 when authentication failed
                    //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.AuthenticationTicket = new Microsoft.AspNet.Authentication.AuthenticationTicket(new ClaimsPrincipal(), new Microsoft.AspNet.Http.Authentication.AuthenticationProperties(), string.Empty);
                    context.HandleResponse();

                    //context.HttpContext.Response.StatusCode = 401;

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
