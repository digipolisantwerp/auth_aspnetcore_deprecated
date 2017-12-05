using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public void ThrowsExceptionIfHttpContextAccessorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(null, Mock.Of<ITokenRefreshAgent>(), Mock.Of<IUrlHelperFactory>(), Mock.Of<IOptions<AuthOptions>>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenRefreshAgentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(Mock.Of<IHttpContextAccessor>(), null, Mock.Of<IUrlHelperFactory>(), Mock.Of<IOptions<AuthOptions>>()));
        }

        [Fact]
        public void ThrowsExceptionIfUrlHelperFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(Mock.Of<IHttpContextAccessor>(), Mock.Of<ITokenRefreshAgent>(), null, Mock.Of<IOptions<AuthOptions>>()));
        }

        [Fact]
        public void GetsUser()
        {
            var user = new ClaimsPrincipal();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext.User)
                .Returns(user);
            var authService = new AuthService(mockHttpContextAccessor.Object, Mock.Of<ITokenRefreshAgent>(), Mock.Of<IUrlHelperFactory>(), Mock.Of<IOptions<AuthOptions>>());

            var returnedUser = authService.User;

            Assert.Same(user, returnedUser);
        }

        [Fact]
        public void GetsUserTokenFromSession()
        {
            var user = new ClaimsPrincipal();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext.User)
                .Returns(user);
            var mockSession = new Mock<ISession>();
            var userToken = "user token";
            var userTokenBytes = Encoding.UTF8.GetBytes(userToken);
            mockSession.Setup(s => s.TryGetValue("auth-jwt", out userTokenBytes)).Returns(true);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.Session).Returns(mockSession.Object);
            mockHttpContextAccessor.SetupGet(c => c.HttpContext).Returns(mockHttpContext.Object);
            var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
            mockAuthOptions.Setup(o => o.Value.JwtTokenSource).Returns("session");
            var mockTokenRefreshAgent = new Mock<ITokenRefreshAgent>();
            var authService = new AuthService(mockHttpContextAccessor.Object, mockTokenRefreshAgent.Object, Mock.Of<IUrlHelperFactory>(), mockAuthOptions.Object);

            var returnedUserToken = authService.UserToken;

            Assert.Equal(userToken, returnedUserToken);
        }

        [Fact]
        public void GetsUserTokenFromHeader()
        {
            var user = new ClaimsPrincipal();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext.User)
                .Returns(user);
            var mockRequest = new Mock<HttpRequest>();
            var userToken = "user token";
            var userTokenBytes = Encoding.UTF8.GetBytes(userToken);
            var mockHeaderDictionary = new Mock<IHeaderDictionary>();
            var userTokenHeader = new Microsoft.Extensions.Primitives.StringValues("Bearer " + userToken);
            mockHeaderDictionary.SetupGet(d => d["Authorization"]).Returns(userTokenHeader);
            mockRequest.SetupGet(s => s.Headers).Returns(mockHeaderDictionary.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.Request).Returns(mockRequest.Object);
            mockHttpContextAccessor.SetupGet(c => c.HttpContext).Returns(mockHttpContext.Object);
            var mockAuthOptions = new Mock<IOptions<AuthOptions>>();
            mockAuthOptions.Setup(o => o.Value.JwtTokenSource).Returns("header");
            var mockTokenRefreshAgent = new Mock<ITokenRefreshAgent>();
            var authService = new AuthService(mockHttpContextAccessor.Object, mockTokenRefreshAgent.Object, Mock.Of<IUrlHelperFactory>(), mockAuthOptions.Object);

            var returnedUserToken = authService.UserToken;

            Assert.Equal(userToken, returnedUserToken);
        }

        [Fact]
        public async Task LogOutAsync()
        {
            var mockAuthenticationManager = new Mock<AuthenticationManager>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(m => m.Authentication)
                .Returns(mockAuthenticationManager.Object);

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(m => m.Scheme)
                .Returns("http");

            mockHttpContext.SetupGet(m => m.Request)
                .Returns(mockRequest.Object);

            mockHttpContext.SetupGet(m => m.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "user123") })));

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext)
                .Returns(mockHttpContext.Object);

            var mockTokenAgent = new Mock<ITokenRefreshAgent>();
            mockTokenAgent.Setup(a => a.LogoutTokenAsync("user123", "logoutUrl"))
                .ReturnsAsync("logoutUrl");

            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();

            UrlActionContext urlActionContext = null;

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<UrlActionContext>()))
                .Returns<UrlActionContext>(_urlActionContext =>
                {
                    urlActionContext = _urlActionContext;
                    return "logoutUrl";
                });

            mockUrlHelperFactory.Setup(m => m.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            var authService = new AuthService(mockHttpContextAccessor.Object, mockTokenAgent.Object, mockUrlHelperFactory.Object, Mock.Of<IOptions<AuthOptions>>());

            var controllerContext = new Mock<ControllerContext>();

            var result = await authService.LogOutAsync(new Microsoft.AspNetCore.Mvc.ControllerContext(), "Home", "Index");

            mockAuthenticationManager.Verify(m => m.SignOutAsync(AuthSchemes.CookieAuth), Times.Once);
            Assert.Equal("logoutUrl", result);
            Assert.Equal("Home", urlActionContext.Controller);
            Assert.Equal("Index", urlActionContext.Action);
            Assert.Equal("http", urlActionContext.Protocol);
        }
    }
}
