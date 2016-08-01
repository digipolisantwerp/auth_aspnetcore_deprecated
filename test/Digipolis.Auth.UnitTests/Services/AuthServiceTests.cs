using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Security.Claims;
using Digipolis.Auth.Services;
using Xunit;

namespace Digipolis.Auth.UnitTests.Services
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
