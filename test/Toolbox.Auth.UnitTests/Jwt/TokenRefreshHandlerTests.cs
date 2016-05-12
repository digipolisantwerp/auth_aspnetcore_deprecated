using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class TokenRefreshHandlerTests
    {
        [Fact]
        public async Task RefreshTokenWhenExpired()
        {
            var options = Options.Create<AuthOptions>(new AuthOptions() { JwtAudience = "audience" });
            var logger = new TestLogger<TokenRefreshHandler>();
            var tokenRefreshAgentMock = new Mock<ITokenRefreshAgent>();

            var tokenRefreshHandler = new TokenRefreshHandler(options, tokenRefreshAgentMock.Object, logger);

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.CreateJwt(new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Expires= DateTime.Now.AddMinutes(-1),
                Audience = "audience"
            });

            await tokenRefreshHandler.HandleRefreshAsync(jwt);

            tokenRefreshAgentMock.Verify(a => a.RefreshTokenAsync(jwt), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenWhenWithinTokenRefreshTime()
        {
            var options = Options.Create<AuthOptions>(new AuthOptions() {  TokenRefreshTime = 5 });
            var logger = new TestLogger<TokenRefreshHandler>();
            var tokenRefreshAgentMock = new Mock<ITokenRefreshAgent>();

            var tokenRefreshHandler = new TokenRefreshHandler(options, tokenRefreshAgentMock.Object, logger);

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.CreateJwt(new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Expires = DateTime.Now.AddMinutes(4)
            });

            await tokenRefreshHandler.HandleRefreshAsync(jwt);

            tokenRefreshAgentMock.Verify(a => a.RefreshTokenAsync(jwt), Times.Once);
        }

        [Fact]
        public async Task DontRefreshTokenWhenNotWithinTokenRefreshTime()
        {
            var options = Options.Create<AuthOptions>(new AuthOptions() { TokenRefreshTime = 5 });
            var logger = new TestLogger<TokenRefreshHandler>();
            var tokenRefreshAgentMock = new Mock<ITokenRefreshAgent>();

            var tokenRefreshHandler = new TokenRefreshHandler(options, tokenRefreshAgentMock.Object, logger);

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.CreateJwt(new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Expires = DateTime.Now.AddMinutes(6)
            });

            await tokenRefreshHandler.HandleRefreshAsync(jwt);

            tokenRefreshAgentMock.Verify(a => a.RefreshTokenAsync(jwt), Times.Never);
        }

        [Fact]
        public async Task DontRefreshTokenWhenAudienceDontMatch()
        {
            var options = Options.Create<AuthOptions>(new AuthOptions() { JwtAudience = "audience"});
            var logger = new TestLogger<TokenRefreshHandler>();
            var tokenRefreshAgentMock = new Mock<ITokenRefreshAgent>();

            var tokenRefreshHandler = new TokenRefreshHandler(options, tokenRefreshAgentMock.Object, logger);

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.CreateJwt(new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Expires = DateTime.Now.AddMinutes(-1)
            });

            await tokenRefreshHandler.HandleRefreshAsync(jwt);

            tokenRefreshAgentMock.Verify(a => a.RefreshTokenAsync(jwt), Times.Never);
        }

        [Fact]
        public async Task DontRefreshTokenWhenTokenIsInvalid()
        {
            var options = Options.Create<AuthOptions>(new AuthOptions());
            var logger = new TestLogger<TokenRefreshHandler>();
            var tokenRefreshAgentMock = new Mock<ITokenRefreshAgent>();

            var tokenRefreshHandler = new TokenRefreshHandler(options, tokenRefreshAgentMock.Object, logger);

            var jwt = "abc.123.456";

            await tokenRefreshHandler.HandleRefreshAsync(jwt);

            tokenRefreshAgentMock.Verify(a => a.RefreshTokenAsync(jwt), Times.Never);
            Assert.Equal(1, logger.LoggedMessages.Count);
            Assert.Contains("Invalid jwt refresh request. token:", logger.LoggedMessages.First());
        }
    }
}
