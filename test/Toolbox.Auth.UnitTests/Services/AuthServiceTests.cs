using Microsoft.AspNet.Http;
using Moq;
using System;
using System.Security.Claims;
using Toolbox.Auth.Services;
using Xunit;

namespace Toolbox.Auth.UnitTests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public void ThrowsExceptionIfHttpContextAccessorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(null));
        }

        [Fact]
        public void GetsUser()
        {
            var user = new ClaimsPrincipal();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.SetupGet(m => m.HttpContext.User)
                .Returns(user);
            var authService = new AuthService(mockHttpContextAccessor.Object);

            var returnedUser = authService.User;

            Assert.Same(user, returnedUser);
        }
    }
}
