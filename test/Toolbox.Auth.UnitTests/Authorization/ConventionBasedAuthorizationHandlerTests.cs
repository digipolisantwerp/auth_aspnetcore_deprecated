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
        public void ThrowsExceptionIfPdpProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(null, Options.Create(_authOptions), Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), null, Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), Options.Create<AuthOptions>(null), Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfAllowedResourceResolverIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConventionBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), Options.Create<AuthOptions>(_authOptions), null));
        }

        [Fact]
        public async Task SucceedWhenPermissionsGranted()
        {
            var allowedResource = "read-resource";
            var mockPdpProvider = CreateMockPolicyDescisionProvider(allowedResource, true);
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResource);

            var handler = new ConventionBasedAuthorizationHandler(mockPdpProvider, Options.Create(_authOptions), mockAllowedResourceResolver);
            var context = CreateAuthorizationContext();
            
            await handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task NotSucceedWhenPermissionsRefused()
        {
            var allowedResource = "read-resource";
            var mockPdpProvider = CreateMockPolicyDescisionProvider(allowedResource, false);
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResource);

            var handler = new ConventionBasedAuthorizationHandler(mockPdpProvider, Options.Create(_authOptions), mockAllowedResourceResolver);
            var context = CreateAuthorizationContext();

            await handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        private IPolicyDescisionProvider CreateMockPolicyDescisionProvider(string allowedResource, bool response)
        {
            var mockPdpProvider = new Mock<IPolicyDescisionProvider>();
            mockPdpProvider.Setup(p => p.HasAccessAsync(_userId, _authOptions.ApplicationName, allowedResource))
                .ReturnsAsync(response);

            return mockPdpProvider.Object;
        }

        private IAllowedResourceResolver CreateMockAllowedResourceResolver(string allowedResource)
        {
            var mockAllowedResourceResolver = new Mock<IAllowedResourceResolver>();
            mockAllowedResourceResolver.Setup(r => r.ResolveFromConvention(It.IsAny<AuthorizationContext>()))
                .Returns(allowedResource);

            return mockAllowedResourceResolver.Object;
        }

        private AuthorizationContext CreateAuthorizationContext()
        {
            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, _userId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationContext(requirements, user, null);
            return context;
        }
    }
}
