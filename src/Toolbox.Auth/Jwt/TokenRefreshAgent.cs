using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
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

        public TokenRefreshAgent(IOptions<AuthOptions> options, ILogger<TokenRefreshAgent> logger)
        {
            _authOptions = options.Value;
            _client = new HttpClient();
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
