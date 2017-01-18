using Digipolis.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public class AddAuthFromOptionsTests : AddAuthBaseTests
    {
        public AddAuthFromOptionsTests()
        {
            Act = services =>
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>();
                mockHostingEnvironment.Setup(h => h.EnvironmentName)
                    .Returns("");

                services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

                services.AddApplicationServices(setup =>
                {
                    setup.ApplicationId = Guid.NewGuid().ToString();
                });

                services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.ApplicationBaseUrl = "ApplicationBaseUrl";
                    options.PdpUrl = "http://test.pdp.be/";
                    options.PdpApiKey = "PdpApiKey";
                    options.PdpCacheDuration = 60;
                    options.UseDotnetKeystore = true;
                    options.DotnetKeystore = "keystoreConnectionString";
                    options.JwtAudience = "audience";
                    options.JwtIssuer = "issuer";
                    options.JwtSigningKeyCacheDuration = 8;
                    options.ApiAuthUrl = "apiauthurl";
                    options.ApiAuthIdpUrl = "apiauthidpurl";
                    options.ApiAuthSpName = "authspname";
                    options.ApiAuthSpUrl = "apiauthspurl";
                    options.ApiAuthTokenRefreshUrl = "apiauthtokenrefreshurl";
                    options.TokenRefreshTime = 5;
                    options.AutomaticTokenRefresh = true;
                    options.TokenCallbackRoute = "custom/tokenendpoint";
                    options.TokenRefreshRoute = "custom/tokenrefresh";
                    options.PermissionsRoute = "custom/permissions";
                    options.AccessDeniedPath = "accessdenied";
                });
                services.AddOptions();
            };
        }

        [Fact]
        public void AdditionalPoliciesAreAdded()
        {
            var services = new ServiceCollection();

            var mockHostingEnvironment = new Mock<IHostingEnvironment>();
            mockHostingEnvironment.Setup(h => h.EnvironmentName)
                .Returns("");

            services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

            services.AddApplicationServices(setup =>
            {
                setup.ApplicationId = Guid.NewGuid().ToString();
            });

            var policiesDictionary = new Dictionary<string, AuthorizationPolicy>();
            var firstPolicy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
            policiesDictionary.Add("FirstPolicy", firstPolicy);

            var secondPolicy = new AuthorizationPolicyBuilder()
                                .RequireClaim("someclaim")
                                .Build();
            policiesDictionary.Add("SecondPolicy", secondPolicy);

            services.AddAuth(options => 
            {
                options.ApplicationName = "AppName";
                options.DotnetKeystore = "keystoreConnectionString";
            }, policiesDictionary);
            services.AddOptions();

            var authorizationOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AuthorizationOptions>>()?.Value;

            var testPolicy = authorizationOptions?.GetPolicy("FirstPolicy");

            Assert.NotNull(testPolicy);
            Assert.NotEmpty(testPolicy.Requirements.Where(r => r.GetType() == typeof(DenyAnonymousAuthorizationRequirement)));

            testPolicy = authorizationOptions?.GetPolicy("SecondPolicy");

            Assert.NotNull(testPolicy);
            Assert.NotEmpty(testPolicy.Requirements.Where(r => r.GetType() == typeof(ClaimsAuthorizationRequirement)));
        }

        [Fact]
        public void DataProtectionKeyStorageIsRegistered()
        {
            //To test if the data protection key storage is registred correctly, no connection string is provided what should result in an exception

            var services = new ServiceCollection();

            var mockHostingEnvironment = new Mock<IHostingEnvironment>();
            mockHostingEnvironment.Setup(h => h.EnvironmentName)
                .Returns("");

            services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

            services.AddApplicationServices(setup =>
            {
                setup.ApplicationId = Guid.NewGuid().ToString();
            });

            ArgumentException exception = null;

            try
            {
                services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.UseDotnetKeystore = true;
                    options.EnableCookieAuth = true;
                });
            }
            catch (ArgumentException ex)
            {
                exception = ex;
            }

            Assert.NotNull(exception);
            Assert.Equal("connectionString", exception.Message);
        }

        [Fact]
        public void DataProtectionKeyStorageIsOnlyRegisteredWhenSetinConfig()
        {
            var services = new ServiceCollection();

            var mockHostingEnvironment = new Mock<IHostingEnvironment>();
            mockHostingEnvironment.Setup(h => h.EnvironmentName)
                .Returns("");

            services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

            services.AddApplicationServices(setup =>
            {
                setup.ApplicationId = Guid.NewGuid().ToString();
            });

            //Act should not produce exception
            services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.EnableCookieAuth = true;
                });
        }
    }
}
