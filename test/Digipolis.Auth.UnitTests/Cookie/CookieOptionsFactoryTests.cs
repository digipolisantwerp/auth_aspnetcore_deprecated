using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Moq;
using Xunit;

namespace Digipolis.Auth.UnitTests.Cookie
{
    public class CookieOptionsFactoryTests
    {
        TestLogger<JwtBearerOptionsFactory> _testLogger = TestLogger<JwtBearerOptionsFactory>.CreateLogger();

        [Fact]
        public void ShouldSetAccessDeniedPath()
        {
            var authOptions = new AuthOptions
            {
                AccessDeniedPath = "accessdenied"
            };

            var mockTokenRefreshHandler = new Mock<ITokenRefreshHandler>();

            var cookieOptionsFactory = new CookieOptionsFactory(mockTokenRefreshHandler.Object, Options.Create(authOptions));

            var options = new CookieAuthenticationOptions();
            cookieOptionsFactory.Setup(options);

            Assert.Equal($"/{authOptions.AccessDeniedPath}", options.AccessDeniedPath);
        }


    }
}
