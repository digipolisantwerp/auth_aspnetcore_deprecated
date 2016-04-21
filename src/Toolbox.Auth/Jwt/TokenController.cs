using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    [Route(AuthOptionsDefaults.TokenCallbackRoute)]
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IJwtSigningKeyProvider _signingKeyProvider;
        private readonly AuthOptions _authOptions;
        private readonly IJwtTokenSignatureValidator _signatureValidator;
        private readonly ISecurityTokenValidator _jwtTokenValidator;

        public TokenController(IOptions<AuthOptions> options,
            IJwtSigningKeyProvider signingKeyProvider,
            IJwtTokenSignatureValidator signatureValidator,
            ISecurityTokenValidator jwtTokenValidator,
            ILogger<TokenController> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (signingKeyProvider == null) throw new ArgumentNullException(nameof(signingKeyProvider), $"{nameof(signingKeyProvider)} cannot be null");
            if (signatureValidator == null) throw new ArgumentNullException(nameof(signatureValidator), $"{nameof(signatureValidator)} cannot be null");
            if (jwtTokenValidator == null) throw new ArgumentNullException(nameof(jwtTokenValidator), $"{nameof(jwtTokenValidator)} cannot be null");
            if (logger == null) throw new ArgumentNullException(nameof(logger), $"{nameof(logger    )} cannot be null");

            _signingKeyProvider = signingKeyProvider;
            _authOptions = options.Value;
            _signatureValidator = signatureValidator;
            _jwtTokenValidator = jwtTokenValidator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string returnUrl)
        {
            var jwt = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$3");
            returnUrl = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$1");

            var validationParameters = TokenValidationParametersFactory.Create(_authOptions, _signatureValidator);
            if (validationParameters.ValidateSignature)
                validationParameters.IssuerSigningKey = await _signingKeyProvider.ResolveSigningKeyAsync(false);

            try
            {
                SecurityToken jwtToken;
                var userPrincipal = _jwtTokenValidator.ValidateToken(jwt, validationParameters, out jwtToken);

                await HttpContext.Authentication.SignInAsync(AuthSchemes.CookieAuth, userPrincipal,
                            new AuthenticationProperties
                            {
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                                IsPersistent = false,
                                AllowRefresh = false
                            });

                HttpContext.Response.Cookies.Append("jwt", jwt);

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Jwt token validation failed. Exception: {ex.ToString()}");

                return RedirectToAction("AccessDenied", "Home");
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
