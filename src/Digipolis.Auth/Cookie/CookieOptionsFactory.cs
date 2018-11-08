using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Digipolis.Auth
{
    internal class CookieOptionsFactory
    {
        private readonly ITokenRefreshHandler _tokenRefreshHandler;
        private readonly AuthOptions _authOptions;

        public CookieOptionsFactory(ITokenRefreshHandler tokenRefreshHandler,
            IOptions<AuthOptions> authOptions)
        {
            if (tokenRefreshHandler == null) throw new ArgumentNullException(nameof(tokenRefreshHandler), $"{nameof(tokenRefreshHandler)} cannot be null");
            if (authOptions == null) throw new ArgumentNullException(nameof(authOptions), $"{nameof(authOptions)} cannot be null");

            _tokenRefreshHandler = tokenRefreshHandler;
            _authOptions = authOptions.Value;
        }

        public void Setup(CookieAuthenticationOptions options)
        {
            options.AccessDeniedPath = new PathString($"/{_authOptions.AccessDeniedPath}");
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async context =>
                {
                    string token = null;
                    if (_authOptions.AddJwtCookie)
                        token = context.Request.Cookies["jwt"];

                    if (_authOptions.AddJwtToSession)
                        token = context.HttpContext.Session.GetString("auth-jwt");

                    if (_authOptions.AutomaticTokenRefresh)
                    {
                        // set token to newly received token
                        token = await _tokenRefreshHandler.HandleRefreshAsync(token);
                    }

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        if (_authOptions.AddJwtCookie)
                            context.Response.Cookies.Append("jwt", token);

                        if (_authOptions.AddJwtToSession)
                            context.HttpContext.Session.SetString("auth-jwt", token);
                    }
                },

                OnRedirectToAccessDenied = context =>
                {
                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.Headers["Location"] = context.RedirectUri;
                        context.Response.StatusCode = 403;
                    }
                    else
                    {
                        context.Response.Redirect(new PathString($"/{_authOptions.AccessDeniedPath}"));
                    }
                    return Task.FromResult<object>(null);
                },

                OnRedirectToLogin = context =>
                {
                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.Headers["Location"] = context.RedirectUri;
                        context.Response.StatusCode = 401;
                    }
                    else
                    {
                        var url = $"{_authOptions.ApiAuthUrl}?idp_url={_authOptions.ApiAuthIdpUrl}&sp_name={_authOptions.ApiAuthSpName}&sp_url={_authOptions.ApiAuthSpUrl}&client_redirect={_authOptions.ApplicationBaseUrl}/{_authOptions.TokenCallbackRoute}?returnUrl=";

                        context.RedirectUri = Uri.EscapeUriString($"{url}{context.Request.Path}{context.Request.QueryString}");
                        context.Response.Redirect(context.RedirectUri);
                    }

                    context.HttpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.CookiePrefix + AuthSchemes.CookieAuth);
                    context.HttpContext.Response.Cookies.Delete(JWTTokenKeys.Cookie);
                    
                    return Task.FromResult<object>(null);
                },
            };
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return (request.Path.StartsWithSegments(new PathString("/api"), StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);

        }
    }
}
