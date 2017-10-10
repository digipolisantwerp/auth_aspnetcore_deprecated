using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Digipolis.Auth.Controllers
{
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly ITokenRefreshHandler _tokenRefreshHandler;
        private readonly AuthOptions _authOptions;
        private readonly ISecurityTokenValidator _jwtTokenValidator;
        private readonly ITokenValidationParametersFactory _tokenValidationParametersFactory;

        public TokenController(IOptions<AuthOptions> options,
            ISecurityTokenValidator jwtTokenValidator,
            ILogger<TokenController> logger,
            ITokenRefreshHandler tokenRefreshHandler,
            ITokenValidationParametersFactory tokenValidationParametersFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (jwtTokenValidator == null) throw new ArgumentNullException(nameof(jwtTokenValidator), $"{nameof(jwtTokenValidator)} cannot be null");
            if (logger == null) throw new ArgumentNullException(nameof(logger), $"{nameof(logger    )} cannot be null");
            if (tokenRefreshHandler == null) throw new ArgumentNullException(nameof(tokenRefreshHandler), $"{nameof(tokenRefreshHandler)} cannot be null");

            _authOptions = options.Value;
            _jwtTokenValidator = jwtTokenValidator;
            _logger = logger;
            _tokenRefreshHandler = tokenRefreshHandler;
            _tokenValidationParametersFactory = tokenValidationParametersFactory;
        }

        
        public async Task<IActionResult> Callback(string returnUrl, string jwt)
        {
            var validationParameters = _tokenValidationParametersFactory.Create();

            try
            {
                SecurityToken jwtToken;
                var userPrincipal = _jwtTokenValidator.ValidateToken(jwt, validationParameters, out jwtToken);

                await HttpContext.Authentication.SignInAsync(AuthSchemes.CookieAuth, userPrincipal,
                            new AuthenticationProperties
                            {
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(_authOptions.CookieAuthLifeTime),
                                IsPersistent = false,
                                AllowRefresh = false
                            });

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Jwt token validation failed. Exception: {ex.ToString()}");

                return Redirect($"/{_authOptions.AccessDeniedPath}");
            }

            if (_authOptions.AddJwtCookie)
                HttpContext.Response.Cookies.Append("jwt", jwt);

            if (_authOptions.AddJwtToSession)
                HttpContext.Session.SetString("auth-jwt", jwt);

            return RedirectToLocal(returnUrl);
        }

        public async Task<IActionResult> Refresh(string token)
        {
            if (String.IsNullOrWhiteSpace(token) && _authOptions.AddJwtToSession)
                token = HttpContext.Session.GetString("auth-jwt");

            if (String.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation($"Token refresh failed. No token found in query params or session.");
                return BadRequest();
            }

            var jwt = new JwtSecurityToken(token);

            if (!String.IsNullOrWhiteSpace(_authOptions.JwtAudience) && !jwt.Audiences.FirstOrDefault().StartsWith(_authOptions.JwtAudience))
            {
                _logger.LogInformation($"Token refresh failed. Token audience not valid.");
                return BadRequest();
            }

            var newToken = await _tokenRefreshHandler.HandleRefreshAsync(token);

            if (newToken != null && _authOptions.AddJwtToSession)
                HttpContext.Session.SetString("auth-jwt", newToken);

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
