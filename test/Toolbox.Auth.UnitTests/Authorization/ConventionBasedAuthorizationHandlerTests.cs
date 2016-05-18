using Microsoft.AspNetCore.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Authorization
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
        public void SucceedWhenPermissionsGranted()
        {
            var requiredPermission = "read-resource";
            var mockRequiredPermissionsResolver = CreatemockRequiredPermissionsResolver(requiredPermission);
            var handler = new ConventionBasedAuthorizationHandler(mockRequiredPermissionsResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, requiredPermission)
                });

            var context = CreateAuthorizationContext(permissionClaims);
            
            handler.Handle(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public void NotSucceedWhenPermissionsRefused()
        {
            var requiredPermission = "read-resource";
            var mockRequiredPermissionsResolver = CreatemockRequiredPermissionsResolver(requiredPermission);
            var handler = new ConventionBasedAuthorizationHandler(mockRequiredPermissionsResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, "otherresource")
                });

            var context = CreateAuthorizationContext(permissionClaims);

            handler.Handle(context);

            Assert.False(context.HasSucceeded);
        }

        private IRequiredPermissionsResolver CreatemockRequiredPermissionsResolver(string requiredPermission)
        {
            var mockRequiredPermissionsResolver = new Mock<IRequiredPermissionsResolver>();
            mockRequiredPermissionsResolver.Setup(r => r.ResolveFromConvention(It.IsAny<AuthorizationContext>()))
                .Returns(requiredPermission);

            return mockRequiredPermissionsResolver.Object;
        }

        private AuthorizationContext CreateAuthorizationContext(List<Claim> claims)
        {
            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            claims.Add(new Claim(ClaimTypes.Name, _userId));
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationContext(requirements, user, null);
            return context;
        }
    }
}
