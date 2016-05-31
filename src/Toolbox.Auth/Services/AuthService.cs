using Microsoft.AspNet.Http;
using System;
using System.Security.Claims;


namespace Toolbox.Auth.Services
{
    public class AuthService : IAuthService
    {
        private IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException($"{nameof(httpContextAccessor)} cannot be null.");

            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal User
        {
            get
            {
                return _httpContextAccessor.HttpContext.User;
            }
        }
    }
}
