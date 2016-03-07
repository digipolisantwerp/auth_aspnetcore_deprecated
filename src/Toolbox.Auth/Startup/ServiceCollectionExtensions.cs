using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds Authentication and Authorization services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">A setup action to customize the AuthOptions options.</param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services, Action<AuthOptions> setupAction)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            services.Configure(setupAction);

            AddAuthorization(services);
            RegisterServices(services);

            return services;
        }

        /// <summary>
        /// Adds Authentication and Authorization services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">A setup action to customize the AuthOptionsJsonFile options.</param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services, Action<AuthOptionsJsonFile> setupAction)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            var options = new AuthOptionsJsonFile();
            setupAction.Invoke(options);

            var builder = new ConfigurationBuilder().AddJsonFile(options.FileName);
            var config = builder.Build();
            var section = config.GetSection(options.Section);
            services.Configure<AuthOptions>(section);

            AddAuthorization(services);
            RegisterServices(services);

            return services;
        }

        private static void AddAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ConventionBased,
                                  policy => policy.Requirements.Add(new ConventionBasedRequirement()));
                options.AddPolicy(Policies.CustomBased,
                                  policy => policy.Requirements.Add(new CustomBasedRequirement()));
            });
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IPolicyDescisionProvider, PolicyDescisionProvider>();
            services.AddSingleton<IAuthorizationHandler, ConventionBasedAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, CustomBasedAuthorizationHandler>();
            services.AddSingleton<IRequiredPermissionsResolver, RequiredPermissionsResolver>();
            services.AddSingleton<PermissionsClaimsTransformer>();
            services.AddSingleton<IJwtSigningKeyProvider, JwtSigningKeyProvider>();
            services.AddSingleton<HttpMessageHandler, HttpClientHandler>();
            services.AddSingleton<IJwtTokenSignatureValidator, JwtTokenSignatureValidator>();
        }
    }
}
