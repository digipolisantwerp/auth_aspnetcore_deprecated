using Microsoft.AspNetCore.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Digipolis.Auth.Authorization;
using Digipolis.Auth.Options;
using Xunit;
using System.Threading.Tasks;

namespace Digipolis.Auth.UnitTests.Authorization
{
    public class ConventionBasedAuthorizationHandlerTests
    {
        private readonly AuthOptions _authOptions;
        private readonly string _userId = "user123";

        public ConventionBasedAuthorizationHandlerTests()
        {
            _authOptions = new AuthOptions
            {
                ApplicationName = "APP"
            };
        }

        [Fact]
        public void ThrowsExceptionIfRequiredPermissionsResolverIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(null));
        }

        [Fact]
        public async Task SucceedWhenPermissionsGranted()
        {
            var requiredPermission = "read-resource";
            var mockRequiredPermissionsResolver = CreatemockRequiredPermissionsResolver(requiredPermission);
            var handler = new ConventionBasedAuthorizationHandler(mockRequiredPermissionsResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, requiredPermission)
                });

            var context = CreateAuthorizationHandlerContext(permissionClaims);
            
            await handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task NotSucceedWhenPermissionsRefused()
        {
            var requiredPermission = "read-resource";
            var mockRequiredPermissionsResolver = CreatemockRequiredPermissionsResolver(requiredPermission);
            var handler = new ConventionBasedAuthorizationHandler(mockRequiredPermissionsResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, "otherresource")
                });

            var context = CreateAuthorizationHandlerContext(permissionClaims);

            await handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        private IRequiredPermissionsResolver CreatemockRequiredPermissionsResolver(string requiredPermission)
        {
            var mockRequiredPermissionsResolver = new Mock<IRequiredPermissionsResolver>();
            mockRequiredPermissionsResolver.Setup(r => r.ResolveFromConvention(It.IsAny<AuthorizationHandlerContext>()))
                .Returns(requiredPermission);

            return mockRequiredPermissionsResolver.Object;
        }

        private AuthorizationHandlerContext CreateAuthorizationHandlerContext(List<Claim> claims)
        {
            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            claims.Add(new Claim(ClaimTypes.Name, _userId));
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationHandlerContext(requirements, user, null);
            return context;
        }
    }
}
