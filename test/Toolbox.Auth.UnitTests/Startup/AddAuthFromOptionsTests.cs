using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Linq;
using System.Net.Http;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using Xunit;

namespace Toolbox.Auth.UnitTests.Startup
{
    public class AddAuthFromOptionsTests
    {
        [Fact]
        private void ActionNullRaisesArgumentException()
        {
            Action<AuthOptions> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddAuth(nullAction));

            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        private void PolicyDescisionProviderIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IPolicyDescisionProvider) &&
                                                     sd.ImplementationType == typeof(PolicyDescisionProvider))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        
            [Fact]
        private void AuthorizePermissionsHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAuthorizationHandler) &&
                                                     sd.ImplementationType == typeof(ConventionBasedAuthorizationHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        
        [Fact]
        private void AllowedResourceResolverIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAllowedResourceResolver) &&
                                                     sd.ImplementationType == typeof(AllowedResourceResolver))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        
        [Fact]
        private void HttpClientHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(HttpMessageHandler) &&
                                                     sd.ImplementationType == typeof(HttpClientHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        private void PermissionsClaimsTransformerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(PermissionsClaimsTransformer) &&
                                                     sd.ImplementationType == typeof(PermissionsClaimsTransformer))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        private void AuthOptionsAreRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "AppName";
                options.PdpUrl = "pdpUrl";
                options.PdpCacheDuration = 25;
                options.JwtAudience = "jwtAudienceUrl";
                options.JwtUserIdClaimType = "sub";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<AuthOptions>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<AuthOptions>;
            Assert.NotNull(configOptions);

            var authOptions = new AuthOptions();
            configOptions.Configure(authOptions);

            Assert.Equal("AppName", authOptions.ApplicationName);
            Assert.Equal("pdpUrl", authOptions.PdpUrl);
            Assert.Equal(25, authOptions.PdpCacheDuration);
            Assert.Equal("jwtAudienceUrl", authOptions.JwtAudience);
            Assert.Equal("sub", authOptions.JwtUserIdClaimType);
        }

        [Fact]
        private void ConventionBasedPolicyIsAdded()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var authorizationOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>()?.Value;

            var conventionBasedPolicy = authorizationOptions?.GetPolicy(Policies.ConventionBased);

            Assert.NotNull(conventionBasedPolicy);
            Assert.NotEmpty(conventionBasedPolicy.Requirements.Where(r => r.GetType() == typeof(ConventionBasedRequirement)));
        }

        [Fact]
        private void CustomBasedPolicyIsAdded()
        {
            var services = new ServiceCollection();
            services.AddAuth(options =>
            {
                options.ApplicationName = "Test";
            });

            var authorizationOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>()?.Value;

            var customBasedPolicy = authorizationOptions?.GetPolicy(Policies.CustomBased);

            Assert.NotNull(customBasedPolicy);
            Assert.NotEmpty(customBasedPolicy.Requirements.Where(r => r.GetType() == typeof(CustomBasedRequirement)));
        }

    }
}
