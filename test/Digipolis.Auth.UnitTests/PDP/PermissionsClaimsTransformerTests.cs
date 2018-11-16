using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Xunit;

namespace Digipolis.Auth.UnitTests.PDP
{
    public class PermissionsClaimsTransformerTests
    {
        private readonly AuthOptions _authOptions;
        private readonly string ApplicationName;
        private readonly string _userId = "user123";

        public PermissionsClaimsTransformerTests()
        {
            _authOptions = new AuthOptions
            {
                ApplicationName = "APP"
            };
            ApplicationName = "APPLICATION";
        }

        [Fact]
        public void ThrowsExceptionIfpermissionApplicationNameProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionsClaimsTransformer(null, Mock.Of<IPolicyDecisionProvider>()));
        }

        [Fact]
        public void ThrowsExceptionIfPolicyDecisionProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionsClaimsTransformer(Mock.Of<IPermissionApplicationNameProvider>(),
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

            var pdpProvider = CreateMockPolicyDecisionProvider(pdpResponse, ApplicationName);

            var transformer = new PermissionsClaimsTransformer(CreateMockPermissionApplicationNameProvider(ApplicationName), pdpProvider);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, _userId), new Claim(ClaimTypes.Name, _userId) }, "Bearer"));

            var result = await transformer.TransformAsync(user);

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

            var pdpProvider = CreateMockPolicyDecisionProvider(pdpResponse, ApplicationName);

            var transformer = new PermissionsClaimsTransformer(CreateMockPermissionApplicationNameProvider(ApplicationName), pdpProvider);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(Claims.Name, _userId), new Claim(ClaimTypes.Name, _userId) }, "Bearer"));

            var result = await transformer.TransformAsync(user);

            Assert.NotNull(result);
            Assert.False(result.HasClaim(c => c.Type == Claims.PermissionsType));
        }

        private IPolicyDecisionProvider CreateMockPolicyDecisionProvider(PdpResponse pdpResponse, string applicationName)
        {
            var mockPdpProvider = new Mock<IPolicyDecisionProvider>();
            mockPdpProvider.Setup(p => p.GetPermissionsAsync(_userId, applicationName))
                .ReturnsAsync(pdpResponse);

            return mockPdpProvider.Object;
        }

        private IPermissionApplicationNameProvider CreateMockPermissionApplicationNameProvider(string applicationName)
        {
            var mock = new Mock<IPermissionApplicationNameProvider>();
            mock.Setup(m => m.ApplicationName(new System.Security.Claims.ClaimsPrincipal())).Returns(applicationName);
            
            return mock.Object;
        }
    }
}
