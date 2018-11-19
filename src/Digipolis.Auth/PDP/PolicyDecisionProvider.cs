using Digipolis.Auth.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public class PolicyDecisionProvider : IPolicyDecisionProvider
    {
        private readonly IMemoryCache _cache;
        private readonly AuthOptions _options;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly HttpClient _client;
        private readonly ILogger<PolicyDecisionProvider> _logger;

        public PolicyDecisionProvider(HttpClient httpClient, IMemoryCache cache, IOptions<AuthOptions> options, ILogger<PolicyDecisionProvider> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient), $"{nameof(httpClient)} cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), $"{nameof(logger)} cannot be null");

            if (_options.PdpCacheDuration > 0)
            {
                _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, _options.PdpCacheDuration, 0) };
            }
        }

        public async Task<PdpResponse> GetPermissionsAsync(string user, string application)
        {
            PdpResponse pdpResponse = null;

            if (_options.PdpCacheDuration > 0)
            {
                pdpResponse = _cache.Get<PdpResponse>(BuildCacheKey(user));

                if (pdpResponse != null)
                    return pdpResponse;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"applications/{application}/users/{user.Replace("@", "%40")}/permissions"))
            {
                using (var response = await _client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                        pdpResponse = await response.Content.ReadAsAsync<PdpResponse>();
                    else
                        _logger.LogCritical($"Impossible to retrieve permissions from {_options.PdpUrl} for {application} / {user}. Response status code: {response.StatusCode}");
                }
            }

            if (_options.PdpCacheDuration > 0 && (pdpResponse?.permissions.Any()).GetValueOrDefault())
                _cache.Set(BuildCacheKey(user), pdpResponse, _cacheOptions);

            return pdpResponse;
        }

        private string BuildCacheKey(string user) => $"pdpResponse-{user}";
    }
}

    
