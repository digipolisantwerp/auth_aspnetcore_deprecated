using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class TokenRefreshAgentTests
    {
        private string _tokenRefreshUrl = "http://test.com";
        private AuthOptions _options;
        private TestLogger<TokenRefreshAgent> _logger = TestLogger<TokenRefreshAgent>.CreateLogger();

        public TokenRefreshAgentTests()
        {
            _options = new AuthOptions { ApiAuthTokenRefreshUrl = _tokenRefreshUrl };
        }

        [Fact]
        public async Task RefreshToken()
        {
            var response = new TokenRefreshResponse
            {
                Jwt = "respondedJwt"
            };

            var mockHandler = new MockMessageHandler<TokenRefreshResponse>(HttpStatusCode.OK, response);
            var agent = new TokenRefreshAgent(Options.Create(_options), _logger, mockHandler);
            var result = await agent.RefreshTokenAsync("token");

            Assert.Equal(response.Jwt, result);
        }

        [Fact]
        public async Task ReturnsNullIfNoSuccess()
        {
            var mockHandler = new MockMessageHandler<TokenRefreshResponse>(HttpStatusCode.InternalServerError, null);
            var agent = new TokenRefreshAgent(Options.Create(_options), _logger, mockHandler);
            var result = await agent.RefreshTokenAsync("token");

            Assert.Null(result);
            Assert.Equal(1, _logger.LoggedMessages.Count);
            Assert.Contains($"Token refresh failed. Response status code: {HttpStatusCode.InternalServerError}", _logger.LoggedMessages.First());
        }
    }
}
