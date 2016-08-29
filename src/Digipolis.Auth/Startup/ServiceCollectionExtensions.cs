using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Digipolis.Auth.Authorization;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Mvc;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.Services;

namespace Digipolis.Auth
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds Authentication and Authorization services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">A setup action to customize the AuthOptions options.</param>
        /// <param name="policies">A optional set of policies to add to the authorization middelware.</param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services, Action<AuthOptions> setupAction, Dictionary<string, AuthorizationPolicy> policies = null)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            services.Configure(setupAction);

            var authOptions = BuildAuthOptions(services);
            ValidateAuthConfig(authOptions);

            AddAuthorization(services, policies, authOptions);
            RegisterServices(services);

            return services;
        }

        /// <summary>
        /// Adds Authentication and Authorization services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">A setup action to customize the AuthOptionsJsonFile options.</param>
        /// <param name="policies">A optional set of policies to add to the authorization middelware.</param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services, Action<AuthOptionsJsonFile> setupAction, Dictionary<string, AuthorizationPolicy> policies = null)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            var options = new AuthOptionsJsonFile();
            setupAction.Invoke(options);

            //var basePath = Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT");

            var builder = new ConfigurationBuilder().SetBasePath(options.BasePath)
                                                    .AddJsonFile(options.FileName);
            var config = builder.Build();
            var section = config.GetSection(options.Section);
            services.Configure<AuthOptions>(section);

            var authOptions = BuildAuthOptions(services);
            ValidateAuthConfig(authOptions);

            AddAuthorization(services, policies, authOptions);
            RegisterServices(services);

            return services;
        }

        private static void AddAuthorization(IServiceCollection services, Dictionary<string, AuthorizationPolicy> policies, AuthOptions authOptions)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            services.AddAuthorization(options =>
            {
                var authenticationScheme = GetAuthenticationScheme(authOptions);

                options.AddPolicy(Policies.ConventionBased,
                                  policy =>
                                  {
                                      policy.AuthenticationSchemes.Add(authenticationScheme);
                                      policy.Requirements.Add(new ConventionBasedRequirement());
                                  });
                options.AddPolicy(Policies.CustomBased,
                                  policy =>
                                  {
                                      policy.AuthenticationSchemes.Add(authenticationScheme);
                                      policy.Requirements.Add(new CustomBasedRequirement());
                                  });

                if (policies != null)
                {
                    foreach (var policy in policies)
                    {
                        options.AddPolicy(policy.Key, policy.Value);
                    }
                }
            });
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IPolicyDescisionProvider, PolicyDescisionProvider>();
            services.AddSingleton<IAuthorizationHandler, ConventionBasedAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, CustomBasedAuthorizationHandler>();
            services.AddSingleton<IRequiredPermissionsResolver, RequiredPermissionsResolver>();
            services.AddSingleton<PermissionsClaimsTransformer>();
            services.AddSingleton<IJwtSigningCertificateProvider, JwtSigningCertificateProvider>();
            services.AddSingleton<HttpMessageHandler, HttpClientHandler>();
            //services.AddSingleton<IJwtTokenSignatureValidator, JwtTokenSignatureValidator>();
            services.AddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            services.AddSingleton<ITokenRefreshAgent, TokenRefreshAgent>();
            services.AddSingleton<ITokenRefreshHandler, TokenRefreshHandler>();
            services.AddSingleton<IAuthService, AuthService>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, AuthActionsOptionsSetup>()); 
        }

        private static void ValidateAuthConfig(AuthOptions options)
        {
            //if (options.EnableCookieAuth && options.EnableJwtHeaderAuth)
            //{
            //    throw new Exception("Invalid Auth configuration. CookieAuth and JwtHeaderAuth schemes are both enabled! You can only use one scheme.");
            //}

        }

        private static string GetAuthenticationScheme(AuthOptions authOptions)
        {
            if (authOptions.EnableCookieAuth)
                return AuthSchemes.CookieAuth;

            if (authOptions.EnableJwtHeaderAuth)
                return AuthSchemes.JwtHeaderAuth;

            return null;
        }

        private static AuthOptions BuildAuthOptions(IServiceCollection services)
        {
            var configureOptions = services.BuildServiceProvider().GetRequiredService<IConfigureOptions<AuthOptions>>();
            var authOptions = new AuthOptions();
            configureOptions.Configure(authOptions);
            return authOptions;
        }
    }
}
