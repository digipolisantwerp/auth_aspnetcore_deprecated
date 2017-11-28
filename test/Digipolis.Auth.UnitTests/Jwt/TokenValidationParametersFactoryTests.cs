using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

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

        [Fact]
        public void ValidateTokenLifetimeShouldBeTrue()
        {
            var authOptions = new AuthOptions();
            var devPermissionsOptions = new DevPermissionsOptions
            {
                Environment = "Testing",
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

            Assert.True(tokenValidationParameters.ValidateLifetime);
        }


        [Fact]
        public void ValidateTokenLifetimeShouldBeFalse()
        {
            var authOptions = new AuthOptions();
            var devPermissionsOptions = new DevPermissionsOptions
            {
                Environment = "Testing",
                ValidateTokenLifetime = false
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

            Assert.False(tokenValidationParameters.ValidateLifetime);
        }

        [Fact]
        public void NameClaimTypeRetrieverShouldUseSubClaimIfPresent()
        {

            var authOptions = new AuthOptions()
            {
                EnableServiceAccountAuthorization = true
            };
            var devPermissionsOptions = new DevPermissionsOptions();
            var jwtSigningKeyProviderMock = new Mock<IJwtSigningKeyResolver>();
            var hostingEnvironmentMock = new Mock<IHostingEnvironment>();

            var tokenValidationParametersFactory = new TokenValidationParametersFactory(Options.Create(authOptions),
                jwtSigningKeyProviderMock.Object,
                Options.Create(devPermissionsOptions),
                hostingEnvironmentMock.Object);

            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);

            var result = tokenValidationParametersFactory.NameClaimTypeRetriever(securityToken, String.Empty);

            Assert.Equal(Claims.Sub, result);
        }

        [Fact]
        public void NameClaimTypeRetrieverShouldUseXConsumerUsernameClaimIfSubNotPresent()
        {

            var authOptions = new AuthOptions()
            {
                EnableServiceAccountAuthorization = true
            };
            var devPermissionsOptions = new DevPermissionsOptions();
            var jwtSigningKeyProviderMock = new Mock<IJwtSigningKeyResolver>();
            var hostingEnvironmentMock = new Mock<IHostingEnvironment>();

            var tokenValidationParametersFactory = new TokenValidationParametersFactory(Options.Create(authOptions),
                jwtSigningKeyProviderMock.Object,
                Options.Create(devPermissionsOptions),
                hostingEnvironmentMock.Object);

            SecurityToken securityToken = new JwtSecurityToken();

            var result = tokenValidationParametersFactory.NameClaimTypeRetriever(securityToken, String.Empty);

            Assert.Equal(Claims.XConsumerUsername, result);
        }

        [Fact]
        public void NameClaimTypeRetrieverShouldUseSubIfSubNotPresent()
        {

            var authOptions = new AuthOptions()
            {
                EnableServiceAccountAuthorization = false
            };
            var devPermissionsOptions = new DevPermissionsOptions();
            var jwtSigningKeyProviderMock = new Mock<IJwtSigningKeyResolver>();
            var hostingEnvironmentMock = new Mock<IHostingEnvironment>();

            var tokenValidationParametersFactory = new TokenValidationParametersFactory(Options.Create(authOptions),
                jwtSigningKeyProviderMock.Object,
                Options.Create(devPermissionsOptions),
                hostingEnvironmentMock.Object);

            SecurityToken securityToken = new JwtSecurityToken();

            var result = tokenValidationParametersFactory.NameClaimTypeRetriever(securityToken, String.Empty);

            Assert.Equal(Claims.Sub, result);
        }
    }
}
