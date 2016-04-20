using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class TokenControllerTests
    {
        private TestLogger<TokenController> _logger = TestLogger<TokenController>.CreateLogger();
        private string _jwtToken = "token";
        private string _redirectUrl = "redirecturl";
        private Mock<AuthenticationManager> _mockAuthenticationManager;
        private Mock<IResponseCookies> _mockCookies;
        private ClaimsPrincipal _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, "userid"), new Claim(ClaimTypes.Name, "userid") }, "Bearer"));

        [Fact]
        public void ThrowsExceptionIfOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(null,
                Mock.Of<IJwtSigningKeyProvider>(),
                Mock.Of<IJwtTokenSignatureValidator>(),
                Mock.Of<ISecurityTokenValidator>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfIJwtSigningKeyProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                null,
                Mock.Of<IJwtTokenSignatureValidator>(),
                Mock.Of<ISecurityTokenValidator>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfIJwtTokenSignatureValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<IJwtSigningKeyProvider>(),
                null,
                Mock.Of<ISecurityTokenValidator>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfSecurityTokenValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<IJwtSigningKeyProvider>(),
                Mock.Of<IJwtTokenSignatureValidator>(),
                null,
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TokenController(Options.Create(new AuthOptions()),
                Mock.Of<IJwtSigningKeyProvider>(),
                Mock.Of<IJwtTokenSignatureValidator>(),
                Mock.Of<ISecurityTokenValidator>(),
                null));
        }

        [Fact]
        public async Task ResolveSigningKeyIfValidateSignatureIsTrue()
        {
            var jwtSigningKeyProvider = new Mock<IJwtSigningKeyProvider>();
            var authOptions = new AuthOptions { JwtSigningKeyProviderUrl = "someurl" };

            var tokenController = new TokenController(Options.Create(authOptions),
                jwtSigningKeyProvider.Object,
                Mock.Of<IJwtTokenSignatureValidator>(),
                Mock.Of<ISecurityTokenValidator>(),
                _logger);

            await tokenController.Index("abc?jwt=123");

            jwtSigningKeyProvider.Verify(p => p.ResolveSigningKeyAsync(false), Times.AtLeastOnce);

        }

        [Fact]
        public async Task DontResolveSigningKeyIfValidateSignatureIsTrue()
        {
            var jwtSigningKeyProvider = new Mock<IJwtSigningKeyProvider>();

            var tokenController = new TokenController(Options.Create(new AuthOptions()),
                jwtSigningKeyProvider.Object,
                Mock.Of<IJwtTokenSignatureValidator>(),
                Mock.Of<ISecurityTokenValidator>(),
                _logger);

            await tokenController.Index("abc?jwt=123");

            jwtSigningKeyProvider.Verify(p => p.ResolveSigningKeyAsync(false), Times.Never);

        }

        [Fact]
        public async Task RedirectToReturnUrlWhenTokenIsValid()
        {
            var tokenController = CreateTokenController(true);

            var result = await tokenController.Index($"{_redirectUrl}?jwt={_jwtToken}");

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

            var result = await tokenController.Index($"{_redirectUrl}?jwt={_jwtToken}");

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", ((RedirectToActionResult)result).ActionName);
            Assert.Equal("Home", ((RedirectToActionResult)result).ControllerName);

            Assert.NotEmpty(_logger.LoggedMessages);
            Assert.StartsWith($"Information, Jwt token validation failed. Exception: System.Exception", _logger.LoggedMessages.First());
        }

        [Fact]
        public async Task RedirectToHomeWhenReturnUrlIsNotLocal()
        {
            var tokenController = CreateTokenController(false);

            var result = await tokenController.Index($"{_redirectUrl}?jwt={_jwtToken}");

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", ((RedirectToActionResult)result).ActionName);
            Assert.Equal("Home", ((RedirectToActionResult)result).ControllerName);
        }

        private TokenController CreateTokenController(bool returnUrlIsLocal)
        {
            SecurityToken securityToken = null;

            var jwtSigningKeyProvider = new Mock<IJwtSigningKeyProvider>();
            var jwtTokenValidator = new Mock<ISecurityTokenValidator>();
            jwtTokenValidator.Setup(v => v.ValidateToken(_jwtToken, It.IsAny<TokenValidationParameters>(), out securityToken))
                .Returns(_claimsPrincipal);

            var tokenController = new TokenController(Options.Create(new AuthOptions()),
                jwtSigningKeyProvider.Object,
                Mock.Of<IJwtTokenSignatureValidator>(),
                jwtTokenValidator.Object,
                _logger);

            var mockHttpContext = new Mock<HttpContext>();
            _mockAuthenticationManager = new Mock<AuthenticationManager>();
            mockHttpContext.SetupGet(c => c.Authentication).Returns(_mockAuthenticationManager.Object);
            var mockHttpResponse = new Mock<HttpResponse>();
            mockHttpContext.SetupGet(c => c.Response).Returns(mockHttpResponse.Object);
            _mockCookies = new Mock<IResponseCookies>();
            mockHttpResponse.SetupGet(r => r.Cookies).Returns(_mockCookies.Object);
            tokenController.ActionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(h => h.IsLocalUrl(_redirectUrl)).Returns(returnUrlIsLocal);
            tokenController.Url = mockUrlHelper.Object;

            return tokenController;
        }
    }
}
