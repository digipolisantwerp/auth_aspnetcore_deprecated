using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Middleware
{
    public class TokenEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenEndpointMiddleware> _logger;
        private readonly IJwtSigningKeyProvider _signingKeyProvider;
        private readonly AuthOptions _authOptions;
        private readonly IJwtTokenSignatureValidator _signatureValidator;

        public TokenEndpointMiddleware(RequestDelegate next, 
            IOptions<AuthOptions> options, 
            IJwtSigningKeyProvider signingKeyProvider, 
            IJwtTokenSignatureValidator signatureValidator,
            ILogger<TokenEndpointMiddleware> logger)
        {
            _next = next;
            _signingKeyProvider = signingKeyProvider;
            _authOptions = options.Value;
            _signatureValidator = signatureValidator;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var returnUrl = context.Request.Query["returnUrl"];

            var jwt = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$3");
            returnUrl = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$1");

            var validationParameters = TokenValidationParametersFactory.Create(_authOptions, _signatureValidator);
            if (validationParameters.ValidateSignature)
                validationParameters.IssuerSigningKey = await _signingKeyProvider.ResolveSigningKeyAsync(false);

            var jwtValidator = new JwtSecurityTokenHandler();

            try
            {
                SecurityToken jwtToken;
                var userPrincipal = jwtValidator.ValidateToken(jwt, validationParameters, out jwtToken);

                await context.Authentication.SignInAsync(AuthSchemes.TokenInCookie, userPrincipal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = false,
                        AllowRefresh = false
                    });

                context.Response.Cookies.Append("jwt", jwt);
                context.Response.Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Jwt token validation failed. Exception: {ex.ToString()}");

                context.Response.Redirect("Home/AccessDenied");
            }
        }
    }
}
