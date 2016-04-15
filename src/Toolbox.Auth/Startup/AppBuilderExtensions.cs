using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using System.Linq;
using System.Net;

namespace Toolbox.Auth
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds Authentication and Authorization to the Microsoft.AspNet.Builder.IApplicationBuilder request execution pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            var authOptions = app.ApplicationServices.GetService<IOptions<AuthOptions>>().Value;
            var signingKeyProvider = app.ApplicationServices.GetService<IJwtSigningKeyProvider>();
            var signatureValidator = app.ApplicationServices.GetService<IJwtTokenSignatureValidator>();
            var logger = app.ApplicationServices.GetService<ILogger<JwtBearerMiddleware>>();

            var jwtBearerOptions = JwtBearerOptionsFactory.Create(authOptions, signingKeyProvider, signatureValidator, logger);
            jwtBearerOptions.AuthenticationScheme = AuthSchemes.TokenInHeader;

            //Add middleware that handles jwt tokens present in Authentication Header
            app.UseJwtBearerAuthentication(jwtBearerOptions);

            //Add middleware that handles authentication cookie
            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = AuthSchemes.TokenInCookie;

                options.AccessDeniedPath = new PathString("/Home/AccessDenied/");
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        return Task.FromResult<object>(null);
                    },

                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.Redirect(options.AccessDeniedPath);
                        return Task.FromResult<object>(null);
                    },

                    OnRedirectToLogin = context =>
                    {
                        var url = $"{authOptions.ApiAuthUrl}?idp_url={authOptions.ApiAuthIdpUrl}&sp_name={authOptions.ApiAuthSpName}&sp_url={authOptions.ApiAuthSpUrl}&client_redirect=http://localhost:51020/token?returnUrl=";

                        context.RedirectUri = Uri.EscapeUriString(url + context.Request.Path);
                        context.Response.Redirect(context.RedirectUri);
                        return Task.FromResult<object>(null);
                    },
                };
            });

            //Add middleware that handles the token endpoint
            app.Map(new PathString("/token"), appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var returnUrl = context.Request.Query["returnUrl"];

                    var jwt = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$3");
                    returnUrl = Regex.Replace(returnUrl, @"(.+)(\?jwt=)(.+)", "$1");

                    var validationParameters = TokenValidationParametersFactory.Create(authOptions, signatureValidator);
                    if(validationParameters.ValidateSignature)
                        validationParameters.IssuerSigningKey = await signingKeyProvider.ResolveSigningKeyAsync(false);

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
                        logger.LogInformation($"Jwt token validation failed. Exception: {ex.ToString()}");

                        context.Response.Redirect("Home/AccessDenied");
                    }
                });
            });

            //Add middleware to set permissions in user claims
            var claimsTransformer = app.ApplicationServices.GetService<PermissionsClaimsTransformer>();
            app.UseClaimsTransformation(options =>
            {
                options.Transformer = claimsTransformer;
            });

            return app;
        }
    }
}
