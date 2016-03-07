using Microsoft.AspNet.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
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
        public void ThrowsExceptionIfAllowedResourceResolverIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(null));
        }

        [Fact]
        public void SucceedWhenPermissionsGranted()
        {
            var allowedResource = "read-resource";
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResource);
            var handler = new ConventionBasedAuthorizationHandler(mockAllowedResourceResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, allowedResource)
                });

            var context = CreateAuthorizationContext(permissionClaims);
            
            handler.Handle(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public void NotSucceedWhenPermissionsRefused()
        {
            var allowedResource = "read-resource";
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResource);
            var handler = new ConventionBasedAuthorizationHandler(mockAllowedResourceResolver);

            var permissionClaims = new List<Claim>(new Claim[]
                {
                    new Claim(Claims.PermissionsType, "otherresource")
                });

            var context = CreateAuthorizationContext(permissionClaims);

            handler.Handle(context);

            Assert.False(context.HasSucceeded);
        }

        private IRequiredPermissionsResolver CreateMockAllowedResourceResolver(string allowedResource)
        {
            var mockAllowedResourceResolver = new Mock<IRequiredPermissionsResolver>();
            mockAllowedResourceResolver.Setup(r => r.ResolveFromConvention(It.IsAny<AuthorizationContext>()))
                .Returns(allowedResource);

            return mockAllowedResourceResolver.Object;
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
