using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;

namespace Digipolis.Auth.Controllers
{
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IJwtSigningKeyProvider _signingKeyProvider;
        private readonly AuthOptions _authOptions;
        private readonly IJwtTokenSignatureValidator _signatureValidator;
        private readonly ISecurityTokenValidator _jwtTokenValidator;
        private readonly ITokenRefreshAgent _tokenRefreshAgent;
        
        public TokenController(IOptions<AuthOptions> options,
            IJwtSigningKeyProvider signingKeyProvider,
            IJwtTokenSignatureValidator signatureValidator,
            ISecurityTokenValidator jwtTokenValidator,
            ILogger<TokenController> logger,
            ITokenRefreshAgent tokenRefreshAgent)
        {
            if (options == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (signingKeyProvider == null) throw new ArgumentNullException(nameof(signingKeyProvider), $"{nameof(signingKeyProvider)} cannot be null");
            if (signatureValidator == null) throw new ArgumentNullException(nameof(signatureValidator), $"{nameof(signatureValidator)} cannot be null");
            if (jwtTokenValidator == null) throw new ArgumentNullException(nameof(jwtTokenValidator), $"{nameof(jwtTokenValidator)} cannot be null");
            if (logger == null) throw new ArgumentNullException(nameof(logger), $"{nameof(logger    )} cannot be null");
            if (tokenRefreshAgent == null) throw new ArgumentNullException(nameof(tokenRefreshAgent), $"{nameof(tokenRefreshAgent)} cannot be null");

            _signingKeyProvider = signingKeyProvider;
            _authOptions = options.Value;
            _signatureValidator = signatureValidator;
            _jwtTokenValidator = jwtTokenValidator;
            _logger = logger;
            _tokenRefreshAgent = tokenRefreshAgent;
        }

        
        public async Task<IActionResult> Callback(string returnUrl, string jwt)
        {
            var validationParameters = TokenValidationParametersFactory.Create(_authOptions, _signatureValidator);
            if (!String.IsNullOrWhiteSpace(_authOptions.JwtSigningKeyProviderUrl))
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

                return Redirect($"/{_authOptions.AccessDeniedPath}");
            }
        }

        public async Task<IActionResult> Refresh(string token)
        {
            var jwt = new JwtSecurityToken(token);

            if (jwt.Audiences.FirstOrDefault() != _authOptions.JwtAudience)
                return BadRequest();

            var newToken = await _tokenRefreshAgent.RefreshTokenAsync(token);

            if (newToken == null)
                return BadRequest();

            return Ok(newToken);
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
