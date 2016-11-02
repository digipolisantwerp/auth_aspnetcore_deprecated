using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Moq;
using System;
using System.Threading.Tasks;
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
