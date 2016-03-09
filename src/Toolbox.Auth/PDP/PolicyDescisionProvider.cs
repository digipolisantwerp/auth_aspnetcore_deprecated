using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.PDP
{
    public class PolicyDescisionProvider : IPolicyDescisionProvider
    {
        private readonly IMemoryCache _cache;
        private readonly AuthOptions _options;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly HttpClient _client;
        private readonly bool cachingEnabled;

        public PolicyDescisionProvider(IMemoryCache cache, IOptions<AuthOptions> options, HttpMessageHandler handler)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            if (options == null || options.Value == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (handler == null) throw new ArgumentNullException(nameof(handler), $"{nameof(handler)} cannot be null");

            _cache = cache;
            _options = options.Value;
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add(HeaderKeys.Apikey, _options.jwtSigningKeyProviderApikey);

            if (_options.PdpCacheDuration > 0)
            {
                cachingEnabled = true;
                _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, _options.PdpCacheDuration, 0) };
            }
        }

        public async Task<PdpResponse> GetPermissions(string user, string application)
        {
            PdpResponse pdpResponse = null;

            if (cachingEnabled)
            {
                pdpResponse = _cache.Get<PdpResponse>(BuildCacheKey(user));

                if (pdpResponse != null)
                    return pdpResponse;
            }

            var response = await _client.GetAsync($"{_options.PdpUrl}/{application}/users/{user}/permissions");
            if (response.IsSuccessStatusCode)
            {
                pdpResponse = await response.Content.ReadAsAsync<PdpResponse>();
            }
            else
            {
                //ToDo log the error
            }

            if (cachingEnabled && pdpResponse != null)
                _cache.Set(BuildCacheKey(user), pdpResponse, _cacheOptions);

            return pdpResponse;
        }

        private string BuildCacheKey(string user) => $"pdpResponse-{user}";
    }
}

    
