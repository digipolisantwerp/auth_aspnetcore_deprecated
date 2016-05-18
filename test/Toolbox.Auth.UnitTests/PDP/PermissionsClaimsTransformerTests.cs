using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using Xunit;

namespace Toolbox.Auth.UnitTests.PDP
{
    public class PermissionsClaimsTransformerTests
    {
        private readonly AuthOptions _authOptions;
        private readonly string _userId = "user123";

        public PermissionsClaimsTransformerTests()
        {
            _authOptions = new AuthOptions
            {
                ApplicationName = "APP"
            };
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionsClaimsTransformer(null, Mock.Of<IPolicyDescisionProvider>()));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionsClaimsTransformer(Options.Create<AuthOptions>(null), 
                Mock.Of<IPolicyDescisionProvider>()));
        }

        [Fact]
        public void ThrowsExceptionIfPolicyDescisionProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionsClaimsTransformer(Options.Create(new AuthOptions()),
               null));
        }

        [Fact]
        public async Task SetClaims()
        {
            var pdpResponse = new PdpResponse
            {
                applicationId = _authOptions.ApplicationName,
                userId = _userId,
                permissions = new List<String>(new string[] { "permission1", "permission2" })
            };

            var pdpProvider = CreateMockPolicyDescisionProvider(pdpResponse);

            var transformer = new PermissionsClaimsTransformer(Options.Create(_authOptions), pdpProvider);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, _userId), new Claim(ClaimTypes.Name, _userId) }, "Bearer"));

            var result = await transformer.TransformAsync(CreateClaimsTransformationContext(user));

            Assert.NotNull(result);
            Assert.True(result.HasClaim(Claims.PermissionsType, "permission1"));
            Assert.True(result.HasClaim(Claims.PermissionsType, "permission2"));
        }

        [Fact]
        public async Task DoesNothingWhenNoPermissionsReturned()
        {
            var pdpResponse = new PdpResponse
            {
                applicationId = _authOptions.ApplicationName,
                userId = _userId,
            };

            var pdpProvider = CreateMockPolicyDescisionProvider(pdpResponse);

            var transformer = new PermissionsClaimsTransformer(Options.Create(_authOptions), pdpProvider);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, _userId), new Claim(ClaimTypes.Name, _userId) }, "Bearer"));

            var result = await transformer.TransformAsync(CreateClaimsTransformationContext(user));

            Assert.NotNull(result);
            Assert.False(result.HasClaim(c => c.Type == Claims.PermissionsType));
        }

        private IPolicyDescisionProvider CreateMockPolicyDescisionProvider(PdpResponse pdpResponse)
        {
            var mockPdpProvider = new Mock<IPolicyDescisionProvider>();
            mockPdpProvider.Setup(p => p.GetPermissionsAsync(_userId, _authOptions.ApplicationName))
                .ReturnsAsync(pdpResponse);

            return mockPdpProvider.Object;
        }

        private ClaimsTransformationContext CreateClaimsTransformationContext(ClaimsPrincipal user)
        {
            var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            mockHttpContext.SetupGet(c => c.User == user);

            return new ClaimsTransformationContext(mockHttpContext.Object);
        }

    }
}
