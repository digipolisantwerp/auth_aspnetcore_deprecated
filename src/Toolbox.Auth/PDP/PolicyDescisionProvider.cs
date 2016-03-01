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

            if (_options.PdpCacheDuration > 0)
            {
                cachingEnabled = true;
                _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, _options.PdpCacheDuration, 0) };
            }
        }

        public async Task<PepResponse> GetPermissions(string user, string application)
        {
            PepResponse pepResponse = null;

            if (cachingEnabled)
            {
                pepResponse = _cache.Get<PepResponse>(BuildCacheKey(user));

                if (pepResponse != null)
                    return pepResponse;
            }

            var response = await _client.GetAsync($"{_options.PdpUrl}/{application}/users/{user}/permissions");
            if (response.IsSuccessStatusCode)
            {
                pepResponse = await response.Content.ReadAsAsync<PepResponse>();
            }
            else
            {
                //ToDo log the error
            }

            if (cachingEnabled && pepResponse != null)
                _cache.Set(BuildCacheKey(user), pepResponse, _cacheOptions);

            return pepResponse;
        }

        private string BuildCacheKey(string user) => $"pdp-{user}";

        public void Dispose()
        {
            _client.Dispose();
            _cache.Dispose();
        }
    }
}

    
