using Digipolis.Auth.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Digipolis.Auth.Services
{
    public class AuthService : IAuthService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenRefreshAgent _tokenRefreshAgent;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AuthService(IHttpContextAccessor httpContextAccessor,
            ITokenRefreshAgent tokenRefreshAgent,
            IUrlHelperFactory urlHelperFactory)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException($"{nameof(httpContextAccessor)} cannot be null.");
            if (tokenRefreshAgent == null) throw new ArgumentNullException($"{nameof(tokenRefreshAgent)} cannot be null.");
            if (urlHelperFactory == null) throw new ArgumentNullException($"{nameof(urlHelperFactory)} cannot be null.");

            _httpContextAccessor = httpContextAccessor;
            _tokenRefreshAgent = tokenRefreshAgent;
            _urlHelperFactory = urlHelperFactory;
        }

        public ClaimsPrincipal User
        {
            get
            {
                return _httpContextAccessor.HttpContext.User;
            }
        }

        public string UserToken
        {
            get
            {
                return _httpContextAccessor.HttpContext.Session.GetString("auth-jwt");
            }
        }

        public async Task<string> LogOutAsync(ControllerContext controllerContext, string redirectController, string redirectAction)
        { 
            await _httpContextAccessor.HttpContext.Authentication.SignOutAsync(AuthSchemes.CookieAuth);

            var urlHelper = _urlHelperFactory.GetUrlHelper(controllerContext);
            var returnUrl = urlHelper.Action(redirectAction, redirectController, null, _httpContextAccessor.HttpContext.Request.Scheme);

            var redirectUrl = await _tokenRefreshAgent.LogoutTokenAsync(User.Identity.Name, returnUrl);

            return redirectUrl;
        }
    }
}
