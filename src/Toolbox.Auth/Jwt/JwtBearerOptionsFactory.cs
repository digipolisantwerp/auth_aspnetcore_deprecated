using Microsoft.AspNet.Authentication.JwtBearer;
using System.IdentityModel.Tokens;
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
                AutomaticAuthenticate = true,
            };

            jwtBearerOptions.TokenValidationParameters.ValidateActor = false;

            jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
            jwtBearerOptions.TokenValidationParameters.ValidAudience = authOptions.JwtAudience;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuer = false;
            jwtBearerOptions.TokenValidationParameters.ValidIssuer = authOptions.JwtIssuer;

            jwtBearerOptions.TokenValidationParameters.ValidateLifetime = false;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey = false;
            jwtBearerOptions.TokenValidationParameters.ValidateSignature = true;
            jwtBearerOptions.TokenValidationParameters.SignatureValidator = signatureValidator.SignatureValidator;


            jwtBearerOptions.TokenValidationParameters.NameClaimType = authOptions.JwtUserIdClaimType;

            jwtBearerOptions.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.FromResult<object>(null);
                },
                OnChallenge = context =>
                {
                    return Task.FromResult<object>(null);
                },
                OnReceivedToken = async context =>
                {
                    context.Options.TokenValidationParameters.IssuerSigningKey = await signingKeyProvider.ResolveSigningKey(true);
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
