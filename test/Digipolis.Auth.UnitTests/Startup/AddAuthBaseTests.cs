using Digipolis.Auth.Authorization;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Authentication;
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
        public Action<IServiceCollection> Act { get; set; }

        [Fact]
        public void ActionNullRaisesArgumentException()
        {
            Action<AuthOptions> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddAuth(nullAction));

            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        public void PolicyDecisionProviderIsRegisteredAsTransient()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IPolicyDecisionProvider))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Transient, registrations[0].Lifetime);
        }


        [Fact]
        public void AuthorizePermissionsHandlerIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAuthorizationHandler) &&
                                                     sd.ImplementationType == typeof(ConventionBasedAuthorizationHandler))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }


        [Fact]
        public void RequiredPermissionsResolverIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IRequiredPermissionsResolver) &&
                                                     sd.ImplementationType == typeof(RequiredPermissionsResolver))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void PermissionsClaimsTransformerIsRegisteredAsScoped()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IClaimsTransformation) &&
                                                     sd.ImplementationType == typeof(PermissionsClaimsTransformer))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        public void AuthOptionsAreRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<AuthOptions>))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<AuthOptions>;
            Assert.NotNull(configOptions);

            var authOptions = new AuthOptions();
            configOptions.Configure(authOptions);

            Assert.Equal("AppName", authOptions.ApplicationName);
            Assert.Equal("ApplicationBaseUrl", authOptions.ApplicationBaseUrl);
            Assert.Equal("http://test.pdp.be/", authOptions.PdpUrl);
            Assert.Equal("PdpApiKey", authOptions.PdpApiKey);
            Assert.True(authOptions.UseDotnetKeystore);
            Assert.Equal("keystoreConnectionString", authOptions.DotnetKeystore);
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
            Assert.Equal(480, authOptions.CookieAuthLifeTime);
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
        public void JwtSigningKeyResolverIsRegisteredAsTransient()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IJwtSigningKeyResolver))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Transient, registrations[0].Lifetime);
        }

        [Fact]
        public void JwtSecurityTokenHandlerIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ISecurityTokenValidator) &&
                                                     sd.ImplementationType == typeof(JwtSecurityTokenHandler))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenRefreshAgentIsRegisteredAsTransient()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenRefreshAgent))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Transient, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenRefreshHandlerIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenRefreshHandler) &&
                                                     sd.ImplementationType == typeof(TokenRefreshHandler))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void TokenValidationParametersFactoryIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenValidationParametersFactory) &&
                                                     sd.ImplementationType == typeof(TokenValidationParametersFactory))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void AuthServiceIsRegisteredAsScoped()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IAuthService) &&
                                                     sd.ImplementationType == typeof(AuthService))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        public void HttpContextAccessorIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IHttpContextAccessor) &&
                                                     sd.ImplementationType == typeof(HttpContextAccessor))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void JwtBearerOptionsFactoryIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(JwtBearerOptionsFactory) &&
                                                     sd.ImplementationType == typeof(JwtBearerOptionsFactory))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void CookieOptionsFactoryIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(CookieOptionsFactory) &&
                                                     sd.ImplementationType == typeof(CookieOptionsFactory))
                                        .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }
    }
}
