using Digipolis.Auth.Controllers;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
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
        private Mock<AuthenticationManager> _mockAuthenticationManager;
        private Mock<IResponseCookies> _mockCookies;
        private Mock<ISession> _mockSession;
        private ClaimsPrincipal _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, "userid"), new Claim(ClaimTypes.Name, "userid") }, "Bearer"));

        [Fact]
        public void ThrowsExceptionIfOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(null,
                Mock.Of<ISecurityTokenValidator>(),
                _logger,
                Mock.Of<ITokenRefreshAgent>(),
                Mock.Of<ITokenValidationParametersFactory>()));
        }

        [Fact]
        public void ThrowsExceptionIfISecurityTokenValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                null,
                _logger,
                Mock.Of<ITokenRefreshAgent>(),
                Mock.Of<ITokenValidationParametersFactory>()));
    }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                null,
                Mock.Of<ITokenRefreshAgent>(),
                Mock.Of<ITokenValidationParametersFactory>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenRefreshAgentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                _logger,
               null,
                Mock.Of<ITokenValidationParametersFactory>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenValidationParametersFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<ISecurityTokenValidator>(),
                null,
                Mock.Of<ITokenRefreshAgent>(),
                null));
        }

        [Fact]
        public async Task RedirectToReturnUrlWhenTokenIsValid()
        {
            var tokenController = CreateTokenController(true);

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            Assert.IsType<RedirectResult>(result);
            Assert.Equal(_redirectUrl, ((RedirectResult)result).Url);

            _mockAuthenticationManager.Verify(a => a.SignInAsync(AuthSchemes.CookieAuth, _claimsPrincipal, It.IsAny<AuthenticationProperties>()));
            _mockCookies.Verify(c => c.Append("jwt", _jwtToken), Times.Once);
        }

        [Fact]
        public async Task RedirectToAccessDeniedWhenTokenIsInValid()
        {
            var tokenController = CreateTokenController(true);

            _mockAuthenticationManager.Setup(m => m.SignInAsync(AuthSchemes.CookieAuth, _claimsPrincipal, It.IsAny<AuthenticationProperties>()))
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
                    .Callback<string, byte[]>((key, value) => addedToken = Encoding.UTF8.GetString(value))
                    .Verifiable();

            var result = await tokenController.Callback(_redirectUrl, _jwtToken);

            _mockCookies.Verify();
            Assert.Equal(addedToken, _jwtToken);
        }

        private TokenController CreateTokenController(bool returnUrlIsLocal, bool disableJwtCookie = false, bool addToSession = false)
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

            var authOptions = new AuthOptions
            {
                AccessDeniedPath = "Home/AccessDenied",
            };

            if (disableJwtCookie) authOptions.AddJwtCookie = false;
            if (addToSession) authOptions.AddJwtToSession = true;

            var tokenController = new TokenController(Options.Create(authOptions),
                jwtTokenValidator.Object,
                _logger,
                Mock.Of<ITokenRefreshAgent>(),
                tokenValidationParametersFactory.Object);

            var mockHttpContext = new Mock<HttpContext>();
            _mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockHttpContext.SetupGet(c => c.Authentication).Returns(_mockAuthenticationManager.Object);
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
