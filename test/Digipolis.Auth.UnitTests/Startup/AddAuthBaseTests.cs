using Digipolis.Auth.Authorization;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public abstract class AddAuthBaseTests
    {
        public Action<ServiceCollection> Act { get; set; }

        [Fact]
        public void ActionNullRaisesArgumentException()
        {
            Action<AuthOptions> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddAuth(nullAction));

            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        public void PolicyDescisionProviderIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IPolicyDescisionProvider) &&
                                                     sd.ImplementationType == typeof(PolicyDescisionProvider))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }


        [Fact]
        public void AuthorizePermissionsHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAuthorizationHandler) &&
                                                     sd.ImplementationType == typeof(ConventionBasedAuthorizationHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }


        [Fact]
        public void RequiredPermissionsResolverIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IRequiredPermissionsResolver) &&
                                                     sd.ImplementationType == typeof(RequiredPermissionsResolver))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }


        [Fact]
        public void HttpClientHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(HttpMessageHandler) &&
                                                     sd.ImplementationType == typeof(HttpClientHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void PermissionsClaimsTransformerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(PermissionsClaimsTransformer) &&
                                                     sd.ImplementationType == typeof(PermissionsClaimsTransformer))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void AuthOptionsAreRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<AuthOptions>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<AuthOptions>;
            Assert.NotNull(configOptions);

            var authOptions = new AuthOptions();
            configOptions.Configure(authOptions);

            Assert.Equal("AppName", authOptions.ApplicationName);
            Assert.Equal("ApplicationBaseUrl", authOptions.ApplicationBaseUrl);
            Assert.Equal("http://test.pdp.be/", authOptions.PdpUrl);
            Assert.Equal("PdpApiKey", authOptions.PdpApiKey);
            Assert.Equal(60, authOptions.PdpCacheDuration);
            Assert.Equal("audience", authOptions.JwtAudience);
            Assert.Equal("issuer", authOptions.JwtIssuer);
            Assert.Equal(8, authOptions.JwtSigningKeyCacheDuration);
            Assert.Equal("apiauthurl", authOptions.ApiAuthUrl);
            Assert.Equal("apiauthidpurl", authOptions.ApiAuthIdpUrl);
            Assert.Equal("authspname", authOptions.ApiAuthSpName);
            Assert.Equal("apiauthspurl", authOptions.ApiAuthSpUrl);
            Assert.Equal("apiauthtokenrefreshurl", authOptions.ApiAuthTokenRefreshUrl);
            Assert.Equal(5, authOptions.TokenRefreshTime);
            Assert.True(authOptions.AutomaticTokenRefresh);
            Assert.Equal("custom/tokenendpoint", authOptions.TokenCallbackRoute);
            Assert.Equal("custom/tokenrefresh", authOptions.TokenRefreshRoute);
            Assert.Equal("custom/permissions", authOptions.PermissionsRoute);
            Assert.Equal("accessdenied", authOptions.AccessDeniedPath); 
        }

        [Fact]
        public void ConventionBasedPolicyIsAdded()
        {
            var services = new ServiceCollection();

            Act(services);

            var authorizationOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>()?.Value;

            var conventionBasedPolicy = authorizationOptions?.GetPolicy(Policies.ConventionBased);

            Assert.NotNull(conventionBasedPolicy);
            Assert.Equal(AuthSchemes.JwtHeaderAuth, conventionBasedPolicy.AuthenticationSchemes.First());
            Assert.NotEmpty(conventionBasedPolicy.Requirements.Where(r => r.GetType() == typeof(ConventionBasedRequirement)));
        }

        [Fact]
        public void CustomBasedPolicyIsAdded()
        {
            var services = new ServiceCollection();

            Act(services);

            var authorizationOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>()?.Value;

            var customBasedPolicy = authorizationOptions?.GetPolicy(Policies.CustomBased);

            Assert.NotNull(customBasedPolicy);
            Assert.Equal(AuthSchemes.JwtHeaderAuth, customBasedPolicy.AuthenticationSchemes.First());
            Assert.NotEmpty(customBasedPolicy.Requirements.Where(r => r.GetType() == typeof(CustomBasedRequirement)));
        }

        [Fact]
        public void JwtSigningKeyResolverIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IJwtSigningKeyResolver) &&
                                                     sd.ImplementationType == typeof(JwtSigningKeyResolver))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void JwtSecurityTokenHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ISecurityTokenValidator) &&
                                                     sd.ImplementationType == typeof(JwtSecurityTokenHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenRefreshAgentIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenRefreshAgent) &&
                                                     sd.ImplementationType == typeof(TokenRefreshAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenRefreshHandlerIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenRefreshHandler) &&
                                                     sd.ImplementationType == typeof(TokenRefreshHandler))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenValidationParametersFactoryIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenValidationParametersFactory) &&
                                                     sd.ImplementationType == typeof(TokenValidationParametersFactory))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void AuthServiceIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAuthService) &&
                                                     sd.ImplementationType == typeof(AuthService))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void HttpContextAccessorIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IHttpContextAccessor) &&
                                                     sd.ImplementationType == typeof(HttpContextAccessor))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void JwtBearerOptionsFactoryIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(JwtBearerOptionsFactory) &&
                                                     sd.ImplementationType == typeof(JwtBearerOptionsFactory))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }
    }
}
