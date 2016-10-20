using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.PdpUrl = "http://test.pdp.be/";
                    options.PdpCacheDuration = 60;
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
    }
}
