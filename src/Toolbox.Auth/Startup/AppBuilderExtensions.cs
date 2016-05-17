using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds Authentication and Authorization to the Microsoft.AspNetCore.Builder.IApplicationBuilder request execution pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            var authOptions = app.ApplicationServices.GetService<IOptions<AuthOptions>>().Value;
            var signingKeyProvider = app.ApplicationServices.GetService<IJwtSigningKeyProvider>();
            var signatureValidator = app.ApplicationServices.GetService<IJwtTokenSignatureValidator>();
            var logger = app.ApplicationServices.GetService<ILogger<JwtBearerMiddleware>>();
            var tokenRefreshHandler = app.ApplicationServices.GetService<ITokenRefreshHandler>();

            var jwtBearerOptions = JwtBearerOptionsFactory.Create(authOptions, signingKeyProvider, signatureValidator, logger);
            jwtBearerOptions.AuthenticationScheme = AuthSchemes.JwtHeaderAuth;

            if (authOptions.EnableJwtHeaderAuth)
            {
                //Add middleware that handles jwt tokens present in Authentication Header
                app.UseJwtBearerAuthentication(jwtBearerOptions);
            }

            if (authOptions.EnableCookieAuth)
            {
                //Add middleware that handles authentication cookie
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = AuthSchemes.CookieAuth,

                    AccessDeniedPath = new PathString($"/{authOptions.AccessDeniedPath}"),
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,

                    Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async context =>
                        {
                            if (authOptions.AutomaticTokenRefresh)
                            {
                                var token = context.Request.Cookies["jwt"];

                                var response = await tokenRefreshHandler.HandleRefreshAsync(token);

                                if (response != null)
                                    context.Response.Cookies.Append("jwt", response);
                            }
                        },

                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.Redirect(new PathString($"/{authOptions.AccessDeniedPath}"));
                            return Task.FromResult<object>(null);
                        },

                        OnRedirectToLogin = context =>
                        {
                            var url = $"{authOptions.ApiAuthUrl}?idp_url={authOptions.ApiAuthIdpUrl}&sp_name={authOptions.ApiAuthSpName}&sp_url={authOptions.ApiAuthSpUrl}&client_redirect=http://{context.Request.Host.Value}/{authOptions.TokenCallbackRoute}?returnUrl=";

                            context.RedirectUri = Uri.EscapeUriString(url + context.Request.Path);
                            context.Response.Redirect(context.RedirectUri);
                            return Task.FromResult<object>(null);
                        },
                    }
                });
            }

            //Add middleware to set permissions in user claims
            var claimsTransformer = app.ApplicationServices.GetService<PermissionsClaimsTransformer>();
            app.UseClaimsTransformation(new ClaimsTransformationOptions
            {
                Transformer = claimsTransformer
            });

            return app;
        }
    }
}
