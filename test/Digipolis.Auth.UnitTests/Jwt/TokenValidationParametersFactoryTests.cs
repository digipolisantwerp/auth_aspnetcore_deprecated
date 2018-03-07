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

        [Fact]
        public void NameClaimTypeRetriever_Should_Use_XAuthenticatedUserId_If_Present()
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

            var token = "eyJ4NXUiOiJodHRwczpcL1wvYXBpLWd3LW8uYW50d2VycGVuLmJlXC9rZXlzXC9wdWIiLCJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE0OTE5MTA5ODMsIlgtQ3JlZGVudGlhbC1Vc2VybmFtZSI6Im5vbmUiLCJYLUNvbnN1bWVyLUdyb3VwcyI6InBla2UubW9ja2Jpbi52MSIsImlzcyI6IjcwOTU5NGMyYjAwZDRkMjA5MDJlZTc3YWZkMDJiYjBjIiwiYXVkIjoibW9ja2Jpbi5vcmciLCJYLUpXVC1Jc3N1ZXIiOiJodHRwczpcL1wvYXBpLWd3LW8uYW50d2VycGVuLmJlIiwiWC1Db25zdW1lci1Vc2VybmFtZSI6ImludC1wZWtlLm1vY2tiaW4udjEiLCJYLUNvbnN1bWVyLUN1c3RvbS1JRCI6ImludC1wZWtlLm1vY2tiaW4udjEiLCJYLUF1dGhlbnRpY2F0ZWQtVXNlcmlkIjoicmMwMDExNUBkaWdhbnQuYW50d2VycGVuLmxvY2FsIiwiWC1BdXRoZW50aWNhdGVkLVNjb3BlIjoibm9uZSIsIlgtSG9zdC1PdmVycmlkZSI6Im5vbmUiLCJpYXQiOjE0OTE5MTEyODMsImp0aSI6IjhiNGJjNTIxNWJmODQ5NmJiOTM1ZjNiMzA1Yzg3NzEzIiwiWC1Db25zdW1lci1JRCI6IjU0ODkwNWU0LWM0OTUtNGUxOS1hOGQ5LTY0NGMwMmJkY2ExYiIsImV4cCI6MTQ5MjEyNzI4M30.MlSg19vT0zi3Vh8k283FzsHaseggezSWFuWN2-n4r-VsOXNuN1mxge95EFz2v_fJ__YN_b2w5CYJ0GKFXSDjD7kxctc3h8m3pI55GyHsDePn66qXipS0ayShaWKAkeg0xGWBV3KuHuGFVmEwcUJbi5yAYhfRqUdNbSSCMS1SuFA-jyOmr_jT7NSJGehjGzby20perBGnVnQhULv0mf3mX1Li3IX4jKHVMOB3dJKnhgazaOhS0pDhiERbTqop1e3H-g6hKttRSkOJNPyLbzw76fJfq9eLLQEGE8_XtU_W8iXy_1Wb6B6Qbao8IMFx65T1xGIALqR556TgWdXjNsAROQCBFNv0aCdbExvxYUjpu_w56JlYqMCRfEcxr1d2h8axxQDJrosu5T2YjjS61k0MXgFpbQqEj5N9Y47kvmp0qN9SQU9bKMsP3Pvw9oixgLNa-TaHvtTjovWl9iw4s4krQtaTlQvtXU5S99ZnQMLPdhZl_2VR3vVS75yoy-UXKENBAEoQRZ2FQfAV_cEBM8q5DGOR-SD17faNaRjIrTqLTRjr4RdXZbmYhQziEmKfG2vVQYjUIjBXINJS7KmiGLn4ZFpqM7jBXn-bmNBRRsmSEAMF4qIExhYavY2gwQ6MeQg4ZfwW7Oto9ce_Oy2fnxOanMPgAyG3GKfLRrm8Brg7i6w";
            SecurityToken securityToken = new JwtSecurityToken(token);

            var result = tokenValidationParametersFactory.NameClaimTypeRetriever(securityToken, String.Empty);

            Assert.Equal(Claims.XAuthenticatedUserId, result);
        }
    }
}
