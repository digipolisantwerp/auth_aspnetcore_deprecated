
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
    public class CustomBasedAuthorizationHandlerTests
    {
        private readonly AuthOptions _authOptions;
        private readonly string _userId = "user123";

        public CustomBasedAuthorizationHandlerTests()
        {
            _authOptions = new AuthOptions
            {
                ApplicationName = "APP"
            };
        }

        [Fact]
        public void ThrowsExceptionIfPdpProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomBasedAuthorizationHandler(null, Options.Create(_authOptions), Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), null, Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), Options.Create<AuthOptions>(null), Mock.Of<IAllowedResourceResolver>()));
        }

        [Fact]
        public void ThrowsExceptionIfAllowedResourceResolverIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomBasedAuthorizationHandler(Mock.Of<IPolicyDescisionProvider>(), Options.Create<AuthOptions>(_authOptions), null));
        }

        [Fact]
        public async Task SucceedWhenPermissionsGranted()
        {
            var allowedResources = new[] { "read-resource" };
            var mockPdpProvider = CreateMockPolicyDescisionProvider(allowedResources, true);
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResources);

            var handler = new CustomBasedAuthorizationHandler(mockPdpProvider, Options.Create(_authOptions), mockAllowedResourceResolver);
            var context = CreateAuthorizationContext();
            
            await handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task NotSucceedWhenPermissionsRefused()
        {
            var allowedResources = new[] { "read-resource" };
            var mockPdpProvider = CreateMockPolicyDescisionProvider(allowedResources, false);
            var mockAllowedResourceResolver = CreateMockAllowedResourceResolver(allowedResources);

            var handler = new CustomBasedAuthorizationHandler(mockPdpProvider, Options.Create(_authOptions), mockAllowedResourceResolver);
            var context = CreateAuthorizationContext();

            await handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        private IPolicyDescisionProvider CreateMockPolicyDescisionProvider(IEnumerable<string> allowedResources, bool response)
        {
            var mockPdpProvider = new Mock<IPolicyDescisionProvider>();
            mockPdpProvider.Setup(p => p.HasAccessAsync(_userId, _authOptions.ApplicationName, allowedResources))
                .ReturnsAsync(response);

            return mockPdpProvider.Object;
        }

        private IAllowedResourceResolver CreateMockAllowedResourceResolver(IEnumerable<string> allowedResources)
        {
            var mockAllowedResourceResolver = new Mock<IAllowedResourceResolver>();
            mockAllowedResourceResolver.Setup(r => r.ResolveFromAttributeProperties(It.IsAny<AuthorizationContext>()))
                .Returns(allowedResources);

            return mockAllowedResourceResolver.Object;
        }

        private AuthorizationContext CreateAuthorizationContext()
        {
            var requirements = new IAuthorizationRequirement[] { new CustomBasedRequirement() };

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
