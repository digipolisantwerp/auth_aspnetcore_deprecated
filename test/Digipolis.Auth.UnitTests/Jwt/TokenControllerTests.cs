using Digipolis.Auth.Controllers;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class TokenControllerTests
    {
        private TestLogger<TokenController> _logger = TestLogger<TokenController>.CreateLogger();
        private string _jwtToken = "token";
        private string _redirectUrl = "redirecturl";
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private Mock<IResponseCookies> _mockCookies;
        private Mock<ISession> _mockSession;
        private ClaimsPrincipal _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, "userid"), new Claim(ClaimTypes.Name, "userid") }, "Bearer"));

        [Fact]
        public void ThrowsExceptionIfOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(null,
                Mock.Of<ISecurityTokenValidator>(),
                _logger,
                Mock.Of<ITokenRefreshHandler>(),
                Mock.Of<ITokenValidationParametersFactory>(),
                Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfISecurityTokenValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                null,
                _logger,
                Mock.Of<ITokenRefreshHandler>(),
                Mock.Of<ITokenValidationParametersFactory>(),
                Mock.Of<IAuthenticationService>()));
    }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                null,
                Mock.Of<ITokenRefreshHandler>(),
                Mock.Of<ITokenValidationParametersFactory>(),
                Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenRefreshAgentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                _logger,
               null,
                Mock.Of<ITokenValidationParametersFactory>(),
                Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenValidationParametersFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                null,
                Mock.Of<ITokenRefreshHandler>(),
                null,
                Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfAuthenticationServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                null,
                Mock.Of<ITokenRefreshHandler>(),
                Mock.Of<ITokenValidationParametersFactory>(),
                null));
        }

        [Fact]
        public async Task RedirectToReturnUrlWhenTokenIsValid()
        {
            var tokenController = CreateTokenController(true);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.IsType<RedirectResult>(result);
            Assert.Equal(_redirectUrl, ((RedirectResult)result).Url);

            _mockAuthenticationService.Verify(a => a.SignInAsync(It.IsAny<HttpContext>(), AuthSchemes.CookieAuth, _claimsPrincipal, It.IsAny<AuthenticationProperties>()));
            _mockCookies.Verify(c => c.Append("jwt", _jwtToken), Times.Once);
        }

        [Fact]
        public async Task RedirectToAccessDeniedWhenTokenIsInValid()
        {
            var tokenController = CreateTokenController(true);

            _mockAuthenticationService.Setup(m => m.SignInAsync(It.IsAny<HttpContext>(), AuthSchemes.CookieAuth, _claimsPrincipal, It.IsAny<AuthenticationProperties>()))
                .Throws<Exception>();

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.IsType<RedirectResult>(result);
            Assert.Equal("/Home/AccessDenied", ((RedirectResult)result).Url);

            Assert.NotEmpty(_logger.LoggedMessages);
            Assert.StartsWith($"Information, Jwt token validation failed. Exception: System.Exception", _logger.LoggedMessages.First());
        }

        [Fact]
        public async Task RedirectToHomeWhenReturnUrlIsNotLocal()
        {
            var tokenController = CreateTokenController(false);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", ((RedirectToActionResult)result).ActionName);
            Assert.Equal("Home", ((RedirectToActionResult)result).ControllerName);
        }

        [Fact]
        public async Task SetJwtCookieByDefault()
        {
            var tokenController = CreateTokenController(true);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            _mockCookies.Verify(c => c.Append("jwt", _jwtToken), Times.Once);
        }

        [Fact]
        public async Task NotSetJwtCookieWhenOptionIsDisabled()
        {
            var tokenController = CreateTokenController(true, disableJwtCookie: true);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            _mockCookies.Verify(c => c.Append("jwt", _jwtToken), Times.Never);
        }

        [Fact]
        public async Task NotAddJwtToSessionByDefault()
        {
            var tokenController = CreateTokenController(true, disableJwtCookie: true);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            _mockSession.Verify(s => s.Set("auth-jwt", It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task AddJwtToSessionWhenOptionIsSet()
        {
            var tokenController = CreateTokenController(true, false, addToSession: true);

            string addedToken = String.Empty;

            _mockSession.Setup(s => s.Set("auth-jwt", It.IsAny<byte[]>()))
                    .Callback<string, byte[]>((key, value) => addedToken = Encoding.UTF8.GetString(value));

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.Equal(addedToken, _jwtToken);
        }

        [Fact]
        public async Task UseCookieAuthLifeTimeFromOptions()
        {
            var tokenController = CreateTokenController(true, true, false, new AuthOptions { CookieAuthLifeTime = 65 });

            AuthenticationProperties usedAuthenticationProperties = null;
                 
            _mockAuthenticationService.Setup(s => s.SignInAsync(It.IsAny<HttpContext>(), AuthSchemes.CookieAuth, It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                    .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((context, schema, principal, properties) => 
                            usedAuthenticationProperties = properties);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.NotNull(usedAuthenticationProperties);
            var expectedExpiresUtc = DateTime.UtcNow.AddMinutes(65);
            Assert.True(usedAuthenticationProperties.ExpiresUtc <= expectedExpiresUtc && usedAuthenticationProperties.ExpiresUtc > expectedExpiresUtc.AddMinutes(-1));
            Assert.False(usedAuthenticationProperties.IsPersistent);
            Assert.False(usedAuthenticationProperties.AllowRefresh);
        }

        private TokenController CreateTokenController(bool returnUrlIsLocal, bool disableJwtCookie = false, bool addToSession = false, AuthOptions authOptions = null)
        {
            SecurityToken securityToken = null;

            var jwtSigningKeyResolver = new Mock<IJwtSigningKeyResolver>();
            var jwtTokenValidator = new Mock<ISecurityTokenValidator>();
            jwtTokenValidator.Setup(v => v.ValidateToken(_jwtToken, It.IsAny<TokenValidationParameters>(), out securityToken))
                .Returns(_claimsPrincipal);

            var tokenValidationParametersFactory = new Mock<ITokenValidationParametersFactory>();
            var tokenValidationParameters = new TokenValidationParameters();
            tokenValidationParametersFactory.Setup(f => f.Create())
                .Returns(tokenValidationParameters);

            authOptions = authOptions ?? new AuthOptions();
            authOptions.AccessDeniedPath = "Home/AccessDenied";

            if (disableJwtCookie) authOptions.AddJwtCookie = false;
            if (addToSession) authOptions.AddJwtToSession = true;

            _mockAuthenticationService = new Mock<IAuthenticationService>();

            var tokenController = new TokenController(Options.Create(authOptions),
                jwtTokenValidator.Object,
                _logger,
                Mock.Of<ITokenRefreshHandler>(),
                tokenValidationParametersFactory.Object,
                _mockAuthenticationService.Object);

            var mockHttpContext = new Mock<HttpContext>();
            
            var mockHttpResponse = new Mock<HttpResponse>();
            mockHttpContext.SetupGet(c => c.Response).Returns(mockHttpResponse.Object);
            _mockSession = new Mock<ISession>();
            mockHttpContext.SetupGet(c => c.Session).Returns(_mockSession.Object);
            _mockCookies = new Mock<IResponseCookies>();
            mockHttpResponse.SetupGet(r => r.Cookies).Returns(_mockCookies.Object);

            var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor());
            tokenController.ControllerContext = new ControllerContext(actionContext);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(h => h.IsLocalUrl(_redirectUrl)).Returns(returnUrlIsLocal);
            tokenController.Url = mockUrlHelper.Object;

            return tokenController;
        }
    }
}
