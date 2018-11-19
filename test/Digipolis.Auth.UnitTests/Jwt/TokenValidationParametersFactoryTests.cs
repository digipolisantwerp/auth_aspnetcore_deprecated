using Moq;
using System;
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

            var token = "eyJ4NXUiOiJodHRwczpcL1wvYXBpLWd3LWEuYW50d2VycGVuLmJlXC9rZXlzXC9wdWIiLCJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1MjA1MTI1MTEsIlgtQ3JlZGVudGlhbC1Vc2VybmFtZSI6Im5vbmUiLCJYLUNvbnN1bWVyLUdyb3VwcyI6ImFzdGFkLmFwcm9maWVsLnYxLCBhYnMuYWJzYmlqbGFnZWFwaS52MSwgYWJzLmFic2Rvc3NpZXJhcGkudjEsIGFjcGFhcy53Y21jb250ZW50bWFuYWdlci52MywgamltbXloYW5ub24udGVzdC52MSwgYWJzLmFic3Byb2Nlc3NhcGkudjEsIGFjcGFhcy53Y21wcm94eS52MyIsImlzcyI6Imh0dHBzOlwvXC9hcGktZ3ctYS5hbnR3ZXJwZW4uYmUiLCJhdWQiOiJyYXN1MTE1NS5ydGUuYW50d2VycGVuLmxvY2FsOjUwMTAyIiwiWC1KV1QtSXNzdWVyIjoiaHR0cHM6XC9cL2FwaS1ndy1hLmFudHdlcnBlbi5iZSIsIlgtQ29uc3VtZXItVXNlcm5hbWUiOiJpbnQtYWJzaW9kLmZvcm1pb2Rsb2NhbC52MSIsIlgtQ29uc3VtZXItQ3VzdG9tLUlEIjoiaW50LWFic2lvZC5mb3JtaW9kbG9jYWwudjEiLCJYLUF1dGhlbnRpY2F0ZWQtVXNlcmlkIjoibm9uZSIsIlgtQXV0aGVudGljYXRlZC1TY29wZSI6Im5vbmUiLCJYLUhvc3QtT3ZlcnJpZGUiOiJub25lIiwiaWF0IjoxNTIwNTEyODExLCJqdGkiOiI2ODBkZmEyNzgxMGU0MDhhYWE3YjcyOGUxMTdlYjk5ZSIsIlgtQ29uc3VtZXItSUQiOiI3OGM5ZDA0Mi03NTNhLTRkNWUtOGMzNS1hYTg2Yjk4OTI2OTIiLCJleHAiOjE1MjA3Mjg4MTF9.XeGnL6a_NDHYl7WHPL3tmrXP1Ga483ZePFD6x-LoJP8znTmYVfbYtQcajADgtZu2x65detsSBb0Z_MnVK7mod3Eec7niyz67UMu2Be86CmTbl-wTtf6i4UZKVcCk6alS-d2ZC6g9Hk66njOecXES998xij4CiKoikGUJ5AdY6FWxhpnOKRFg5FbhiIpHt294Hf2QSHjuV2476xbYTWCSOr8A61LibPzhR9hOHaopOivnCOkPeJEZtn98Lyoa6CqcM44gdppZdO9rqQ1pxhLrycdsc2dSZ0yxHXwmZ5XwOfmCqbQRIl4WbmMpuMVfUEetHhuZ95pI_oUxZsIqxyMLRwOv6z1K184MVgRzFB6ziZY495a2zXoOMXwqhk7C-Zih_8mgPvYbsoR6Rv7jp95c7xfMTMUDuj0HelIr3FVlff-J7XZEuWeTDdp6Hvk4JkxMnkMiYkeuzmsiJKaCcirUlelSftcebr2AL_-jVQ9jtnfmXt7NUWKrYEd9Ohn7qyxaDWb2M-JzL_FhONt1H81zpzwq45SSC21YjjxNpo9basMtiYzRTtRNMusUbkEHxgwkfLcBNywse0vygWWnehWkD3rwryJAjL5ifrCWEKbC9Bk2BVf1i_kzdCI_iCixy5HXQcQ6bsTtUB_6k1XowsDj9kL7PLAe2nPCErxd7SYA2dg";
            SecurityToken securityToken = new JwtSecurityToken(token);

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
