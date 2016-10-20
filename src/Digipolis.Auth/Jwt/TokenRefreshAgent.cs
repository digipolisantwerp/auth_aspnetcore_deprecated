using Digipolis.Auth.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace Digipolis.Auth.Jwt
{
    public class TokenRefreshAgent : ITokenRefreshAgent
    {
        private readonly AuthOptions _authOptions;
        private readonly HttpClient _client;
        private readonly JsonSerializerSettings _jsonSettings;

        private readonly ILogger<TokenRefreshAgent> _logger;

        public TokenRefreshAgent(IOptions<AuthOptions> options, ILogger<TokenRefreshAgent> logger, HttpMessageHandler handler)
        {
            _authOptions = options.Value;
            _client = new HttpClient(handler);
            //_client.DefaultRequestHeaders.Accept.Add("Content-Type", "application/json");
            _logger = logger;

            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task<string> LogoutTokenAsync(string userName, string redirectUrl)
        {
            var tokenLogoutRequest = new TokenLogoutRequest
            {
                SpName = _authOptions.ApiAuthSpName,
                IdpUrl = _authOptions.ApiAuthIdpUrl,
                Username = userName,
                RelayState = redirectUrl
            };

            var response = await _client.PostAsync<TokenLogoutRequest>(_authOptions.ApiAuthTokenLogoutUrl, tokenLogoutRequest, _jsonSettings);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Token logout failed. Response status code: {response.StatusCode}");
                return null;
            }

            var tokenLogoutResponse = await response.Content.ReadAsStringAsync();
            return tokenLogoutResponse;
        }

        public async Task<string> RefreshTokenAsync(string token)
        {
            var tokenRefreshRequest = new TokenRefreshRequest
            {
                OriginalJWT = token
            };

            var response = await _client.PostAsync<TokenRefreshRequest>(_authOptions.ApiAuthTokenRefreshUrl, tokenRefreshRequest, _jsonSettings);

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
