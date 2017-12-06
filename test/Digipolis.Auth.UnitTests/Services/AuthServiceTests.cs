using Digipolis.Auth.Jwt;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
            Assert.Throws<ArgumentNullException>(() => new AuthService(null, Mock.Of<ITokenRefreshAgent>(), 
                Mock.Of<IUrlHelperFactory>(), Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfTokenRefreshAgentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(Mock.Of<IHttpContextAccessor>(), null, 
                Mock.Of<IUrlHelperFactory>(), Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfUrlHelperFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(Mock.Of<IHttpContextAccessor>(), Mock.Of<ITokenRefreshAgent>(), 
                null, Mock.Of<IAuthenticationService>()));
        }

        [Fact]
        public void ThrowsExceptionIfAuthenticationServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(Mock.Of<IHttpContextAccessor>(), Mock.Of<ITokenRefreshAgent>(),
                Mock.Of<IUrlHelperFactory>(), null));
        }

        [Fact]
        public void GetsUser()
        {
            var user = new ClaimsPrincipal();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext.User)
                .Returns(user);
            var authService = new AuthService(mockHttpContextAccessor.Object, Mock.Of<ITokenRefreshAgent>(), Mock.Of<IUrlHelperFactory>(), Mock.Of<IAuthenticationService>());

            var returnedUser = authService.User;

            Assert.Same(user, returnedUser);
        }

        [Fact]
        public void GetsUserToken()
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
            var authService = new AuthService(mockHttpContextAccessor.Object, Mock.Of<ITokenRefreshAgent>(), Mock.Of<IUrlHelperFactory>(), Mock.Of<IAuthenticationService>());

            var returnedUserToken = authService.UserToken;

            Assert.Equal(userToken, returnedUserToken);
        }

        [Fact]
        public async Task LogOutAsync()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService>();
            var mockHttpContext = new Mock<HttpContext>();

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

            var authService = new AuthService(mockHttpContextAccessor.Object, mockTokenAgent.Object, mockUrlHelperFactory.Object, mockAuthenticationService.Object);

            var controllerContext = new Mock<ControllerContext>();

            var result = await authService.LogOutAsync(new Microsoft.AspNetCore.Mvc.ControllerContext(), "Home", "Index");

            mockAuthenticationService.Verify(m => m.SignOutAsync(mockHttpContext.Object, AuthSchemes.CookieAuth, It.IsAny<AuthenticationProperties>()), Times.Once);
            Assert.Equal("logoutUrl", result);
            Assert.Equal("Home", urlActionContext.Controller);
            Assert.Equal("Index", urlActionContext.Action);
            Assert.Equal("http", urlActionContext.Protocol);
        }
    }
}
