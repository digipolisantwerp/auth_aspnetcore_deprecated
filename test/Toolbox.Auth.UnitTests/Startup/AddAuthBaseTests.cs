 using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using Xunit;

namespace Toolbox.Auth.UnitTests.Startup
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
            Assert.Equal("http://test.pdp.be/", authOptions.PdpUrl);
            Assert.Equal(60, authOptions.PdpCacheDuration);
            Assert.Equal("audience", authOptions.JwtAudience);
            Assert.Equal("issuer", authOptions.JwtIssuer);
            Assert.Equal("singingKeyProviderUrl", authOptions.JwtSigningKeyProviderUrl);
            Assert.Equal("singinKeyProviderApiKey", authOptions.JwtSigningKeyProviderApikey);
            Assert.Equal(8, authOptions.JwtSigningKeyCacheDuration);
            Assert.Equal(3, authOptions.JwtValidatorClockSkew);
            Assert.Equal("apiauthurl", authOptions.ApiAuthUrl);
            Assert.Equal("apiauthidpurl", authOptions.ApiAuthIdpUrl);
            Assert.Equal("authspname", authOptions.ApiAuthSpName);
            Assert.Equal("apiauthspurl", authOptions.ApiAuthSpUrl);
            Assert.Equal("custom/tokenendpoint", authOptions.TokenCallbackRoute);
            Assert.Equal("apiauthtokenrefreshurl", authOptions.ApiAuthTokenRefreshUrl);
            Assert.Equal(5, authOptions.TokenRefreshTime);
            Assert.True(authOptions.AutomaticTokenRefresh);
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
        public void JwtSecurityKeyProviderIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IJwtSigningKeyProvider) &&
                                                     sd.ImplementationType == typeof(JwtSigningKeyProvider))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void JwtTokenSignatureValidatorIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IJwtTokenSignatureValidator) &&
                                                     sd.ImplementationType == typeof(JwtTokenSignatureValidator))
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
        public void TokenControllerOptionsSetupIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<MvcOptions>) &&
                                                     sd.ImplementationType == typeof(TokenControllerOptionsSetup))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Transient, registrations[0].Lifetime);
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
    }
}
