using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    public class TokenRefreshAgent : ITokenRefreshAgent
    {
        private readonly AuthOptions _authOptions;
        private readonly HttpClient _client;
        private readonly JsonMediaTypeFormatter _formatter;
        private readonly ILogger<TokenRefreshAgent> _logger;

        public TokenRefreshAgent(IOptions<AuthOptions> options, ILogger<TokenRefreshAgent> logger, HttpMessageHandler handler)
        {
            _authOptions = options.Value;
            _client = new HttpClient(handler);
            _logger = logger;

            _formatter = new JsonMediaTypeFormatter();
            _formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public async Task<string> RefreshTokenAsync(string token)
        {
            var tokenRefreshRequest = new TokenRefreshRequest
            {
                OriginalJWT = token
            };

            var response = await _client.PostAsync<TokenRefreshRequest>(_authOptions.ApiAuthTokenRefreshUrl, tokenRefreshRequest, _formatter);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Token refresh failed. Response status code: {response.StatusCode}");
                return null;  
            }

            var tokenRefreshResponse = await response.Content.ReadAsAsync<TokenRefreshResponse>();
            return tokenRefreshResponse.Jwt;
        }
    }
}
