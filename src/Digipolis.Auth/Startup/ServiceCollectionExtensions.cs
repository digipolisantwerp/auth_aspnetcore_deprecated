using Digipolis.ApplicationServices;
using Digipolis.Auth.Authorization;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Mvc;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.Services;
using Digipolis.Auth.Utilities;
using Digipolis.DataProtection.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            services.Configure<DevPermissionsOptions>(options => { });

            return AddAuth(services, policies);
        }

        /// <summary>
        /// Adds Authentication and Authorization services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAuthOptions">A setup action to customize the AuthOptions options.</param>
        /// <param name="setupDevPermissions">A setup action to customize the DevPermissionsOptions options.</param>
        /// <param name="policies"></param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services,
            Action<AuthOptions> setupAuthOptions,
            Action<DevPermissionsOptions> setupDevPermissions,
            Dictionary<string, AuthorizationPolicy> policies = null)
        {
            if (setupAuthOptions == null) throw new ArgumentNullException(nameof(setupAuthOptions), $"{nameof(setupAuthOptions)} cannot be null.");
            if (setupDevPermissions == null) throw new ArgumentNullException(nameof(setupDevPermissions), $"{nameof(setupDevPermissions)} cannot be null.");

            services.Configure(setupAuthOptions);
            services.Configure(setupDevPermissions);

            return AddAuth(services, policies);
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

            var builder = new ConfigurationBuilder().SetBasePath(options.BasePath)
                                                    .AddJsonFile(options.FileName);
            var config = builder.Build();
            var section = config.GetSection(options.Section);
            services.Configure<AuthOptions>(section);

            section = config.GetSection("DevPermissions");
            services.Configure<DevPermissionsOptions>(section);

            return AddAuth(services, policies);
        }

        private static IServiceCollection AddAuth(this IServiceCollection services, Dictionary<string, AuthorizationPolicy> policies)
        {
            var serviceProvider = services.BuildServiceProvider();

            var authOptions = BuildOptions<AuthOptions>(serviceProvider, services);
            var devPermissionsOptions = BuildOptions<DevPermissionsOptions>(serviceProvider, services);
            var applicationContext = GetApplicationContext(serviceProvider);

            if (authOptions.EnableCookieAuth && authOptions.UseDotnetKeystore)
            {
                services.AddDataProtection().PersistKeysToPostgres(authOptions.DotnetKeystore, Guid.Parse(applicationContext.ApplicationId), Guid.Parse(applicationContext.InstanceId));
            }

            RegisterServices(services, devPermissionsOptions);
            AddAuthorization(services, policies, authOptions);

            return services;
        }

        private static void AddAuthorization(IServiceCollection services, Dictionary<string, AuthorizationPolicy> policies, AuthOptions authOptions)
        {
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = AuthSchemes.CookieAuth;
            });

            if (authOptions.EnableCookieAuth)
            {
                var cookieOptionsFactory = services.BuildServiceProvider().GetService<CookieOptionsFactory>();
                authenticationBuilder.AddCookie(AuthSchemes.CookieAuth, options =>
                    {
                        cookieOptionsFactory.Setup(options);
                    });
            }

            if (authOptions.EnableJwtHeaderAuth)
            {
                var jwtBearerOptionsFactory = services.BuildServiceProvider().GetService<JwtBearerOptionsFactory>();
                authenticationBuilder.AddJwtBearer(AuthSchemes.JwtHeaderAuth, options =>
                {
                    jwtBearerOptionsFactory.Setup(options);
                });
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ConventionBased,
                                  policy =>
                                  {
                                      policy.AuthenticationSchemes.Add(AuthSchemes.JwtHeaderAuth);
                                      policy.Requirements.Add(new ConventionBasedRequirement());
                                  });
                options.AddPolicy(Policies.CustomBased,
                                  policy =>
                                  {
                                      policy.AuthenticationSchemes.Add(AuthSchemes.JwtHeaderAuth);
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

        private static void RegisterServices(IServiceCollection services, DevPermissionsOptions devPermissionsOptions)
        {
            services.AddMemoryCache();

            services.AddSingleton<IAuthorizationHandler, ConventionBasedAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, CustomBasedAuthorizationHandler>();
            services.AddSingleton<IRequiredPermissionsResolver, RequiredPermissionsResolver>();
            services.AddSingleton<PermissionsClaimsTransformer>();
            services.AddSingleton<IJwtSigningKeyResolver, JwtSigningKeyResolver>();
            services.AddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            services.AddSingleton<ITokenRefreshAgent, TokenRefreshAgent>();
            services.AddSingleton<ITokenRefreshHandler, TokenRefreshHandler>();
            services.AddSingleton<ITokenValidationParametersFactory, TokenValidationParametersFactory>();
            services.AddSingleton<JwtBearerOptionsFactory>();
            services.AddSingleton<CookieOptionsFactory>();
            services.AddSingleton<IClaimsTransformation, PermissionsClaimsTransformer>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<HttpMessageHandler, HttpClientHandler>();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, AuthActionsOptionsSetup>());

            services.AddScoped<IAuthService, AuthService>();

            if (EnvironmentHelper.IsDevelopmentOrRequiredEnvironment(services, devPermissionsOptions.Environment) && devPermissionsOptions.UseDevPermissions)
            {
                services.AddSingleton<IPolicyDescisionProvider, DevPolicyDescisionProvider>();
            }
            else
            {
                services.AddSingleton<IPolicyDescisionProvider, PolicyDescisionProvider>();
            }

            services.TryAddScoped<IPermissionApplicationNameProvider, DefaultPermissionApplicationNameProvider>();
        }

        private static T BuildOptions<T>(IServiceProvider serviceProvider, IServiceCollection services) where T : class, new()
        {
            var configureOptions = serviceProvider.GetRequiredService<IConfigureOptions<T>>();
            var options = new T();
            configureOptions.Configure(options);
            return options;
        }

        private static IApplicationContext GetApplicationContext(IServiceProvider serviceProvider)
        {
            try
            {
                return serviceProvider.GetRequiredService<IApplicationContext>();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"{ex.Message}, make sure you have added and configured the Digipolis.ApplicationContext package!");
            }
        }
    }
}
