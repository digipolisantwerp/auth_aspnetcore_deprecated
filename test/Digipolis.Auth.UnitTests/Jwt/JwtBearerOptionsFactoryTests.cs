using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Text;
using System.Threading.Tasks;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class JwtBearerOptionsFactoryTests
    {
        IJwtSigningKeyResolver signingKeyResolverMock = Mock.Of<IJwtSigningKeyResolver>();
        TestLogger<JwtBearerMiddleware> loggerMock = TestLogger<JwtBearerMiddleware>.CreateLogger();

        [Fact]
        public void CreateJwtBearerOptions()
        {
            var authOptions = new AuthOptions
            {
               JwtIssuer = "jwtIssuer"
            };

            var options = JwtBearerOptionsFactory.Create(authOptions, signingKeyResolverMock, loggerMock);

            Assert.False(options.TokenValidationParameters.ValidateIssuer);
            Assert.Equal(authOptions.JwtIssuer, options.TokenValidationParameters.ValidIssuer);

            Assert.False(options.TokenValidationParameters.ValidateAudience);
            Assert.Equal(authOptions.JwtAudience, options.TokenValidationParameters.ValidAudience);

            Assert.True(options.TokenValidationParameters.ValidateLifetime);

            Assert.Equal(Claims.Sub, options.TokenValidationParameters.NameClaimType);
        }

        //[Fact]
        //public async Task SigningKeyIsSetWhenTokenReceived()
        //{
        //    var keyBytes = Encoding.UTF8.GetBytes("secret");
        //    var authOptions = new AuthOptions { JwtSigningKeyProviderUrl = "jwtSigningKeyProviderUrl" };
        //    var signingKeyProviderMock = new Mock<IJwtSigningCertificateProvider>();
        //    signingKeyProviderMock.Setup(v => v.ResolveSigningKeyAsync(true))
        //        .ReturnsAsync(new SymmetricSecurityKey(keyBytes));

        //    var options = JwtBearerOptionsFactory.Create(authOptions, signingKeyProviderMock.Object, loggerMock);
        //    var context = new MessageReceivedContext(null, options);

        //    await options.Events.MessageReceived(context);

        //    var securityKey = options.TokenValidationParameters.IssuerSigningKey as SymmetricSecurityKey;

        //    Assert.Equal(keyBytes, securityKey.Key);
        //}

        [Fact]
        public async Task LogWhenAuthenticationFailed()
        {
            var authOptions = new AuthOptions();

            var options = JwtBearerOptionsFactory.Create(authOptions, signingKeyResolverMock, loggerMock);
            var context = new AuthenticationFailedContext(null, options);
            context.Exception = new Exception("exceptiondetail");

            await options.Events.AuthenticationFailed(context);

            Assert.NotEmpty(loggerMock.LoggedMessages);
            Assert.Contains("exceptiondetail", loggerMock.LoggedMessages[0]);
        }

        [Fact]
        public async Task EmptyAuthenticationTicketIsSetWHenAuthenticationFailed()
        {
            var authOptions = new AuthOptions();

            var options = JwtBearerOptionsFactory.Create(authOptions, signingKeyResolverMock, loggerMock);
            var context = new AuthenticationFailedContext(null, options);
            context.Exception = new Exception("exceptiondetail");

            await options.Events.AuthenticationFailed(context);

            Assert.True(context.HandledResponse);
            Assert.NotNull(context.Ticket);
        }
    }
}   
