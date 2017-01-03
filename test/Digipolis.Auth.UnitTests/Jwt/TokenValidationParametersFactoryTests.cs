using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class TokenValidationParametersFactoryTests
    {
        [Fact]
        public void ShouldCreateParameters()
        {
            var authOptions = new AuthOptions
            {
                JwtIssuer = "jwtIssuer"
            };

            var devPermissionsOptions = new DevPermissionsOptions();

            var jwtSigningKeyProviderMock = new Mock<IJwtSigningKeyResolver>();
            var hostingEnvironmentMock = new Mock<IHostingEnvironment>();

            var tokenValidationParametersFactory = new TokenValidationParametersFactory(Options.Create(authOptions), 
                jwtSigningKeyProviderMock.Object, 
                Options.Create(devPermissionsOptions), 
                hostingEnvironmentMock.Object);

            var tokenValidationParameters = tokenValidationParametersFactory.Create();

            Assert.False(tokenValidationParameters.ValidateIssuer);
            Assert.Equal(authOptions.JwtIssuer, tokenValidationParameters.ValidIssuer);

            Assert.False(tokenValidationParameters.ValidateAudience);
            Assert.Equal(authOptions.JwtAudience, tokenValidationParameters.ValidAudience);

            Assert.True(tokenValidationParameters.ValidateLifetime);

            Assert.True(tokenValidationParameters.RequireSignedTokens);

            Assert.Equal(Claims.Sub, tokenValidationParameters.NameClaimType);
        }

        [Fact]
        public void RequireSignedTokensShouldBeFalse()
        {
            var authOptions = new AuthOptions();
            var devPermissionsOptions = new DevPermissionsOptions
            {
                Environment = "Testing",
                RequireSignedTokens = false
            };

            var jwtSigningKeyProviderMock = new Mock<IJwtSigningKeyResolver>();
            var hostingEnvironmentMock = new Mock<IHostingEnvironment>();
            hostingEnvironmentMock.SetupGet(h => h.EnvironmentName)
                .Returns(devPermissionsOptions.Environment);

            var tokenValidationParametersFactory = new TokenValidationParametersFactory(Options.Create(authOptions),
                jwtSigningKeyProviderMock.Object,
                Options.Create(devPermissionsOptions),
                hostingEnvironmentMock.Object);

            var tokenValidationParameters = tokenValidationParametersFactory.Create();

            Assert.False(tokenValidationParameters.RequireSignedTokens);
        }

    }
}
