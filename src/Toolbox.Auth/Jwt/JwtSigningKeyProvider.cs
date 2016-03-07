using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    public class JwtSigningKeyProvider : IJwtSigningKeyProvider
    {
        private readonly IMemoryCache _cache;
        private readonly AuthOptions _options;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly HttpClient _client;
        private readonly bool _cachingEnabled;
        private const string CACHE_KEY = "JwtSigningKey";

        public JwtSigningKeyProvider(IMemoryCache cache, IOptions<AuthOptions> options, HttpMessageHandler handler)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            if (options == null || options.Value == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (handler == null) throw new ArgumentNullException(nameof(handler), $"{nameof(handler)} cannot be null");

            _cache = cache;
            _options = options.Value;
            _client = new HttpClient(handler, true);

            if (_options.JwtSigningKeyCacheDuration > 0)
            {
                _cachingEnabled = true;
                _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, _options.JwtSigningKeyCacheDuration, 0) };
            }

            if (_cachingEnabled)
            {
                //Preload singing key on startup
                ResolveSigningKey(true).Wait();
            }
        }

        public async Task<SecurityKey> ResolveSigningKey(bool allowCached)
        {
            SecurityKey signingKey = null;

            if (allowCached && _cachingEnabled)
            {
                signingKey = _cache.Get(CACHE_KEY) as SecurityKey;

                if (signingKey != null)
                    return signingKey;
            }

            var response = await _client.GetAsync(_options.JwtSigningKeyProviderUrl);
            if (response.IsSuccessStatusCode)
            {
                var keyString = await response.Content.ReadAsStringAsync();
                byte[] keyBytes = Encoding.UTF8.GetBytes(keyString);
                //if (keyBytes.Length < 64)
                //    Array.Resize(ref keyBytes, 64);
                signingKey = new SymmetricSecurityKey(keyBytes);
            }
            else
            {
                //ToDo log the error
                //throw exception ??
            }

            if (_cachingEnabled)
                _cache.Set(CACHE_KEY, signingKey, _cacheOptions);

            return signingKey;
        }
    }
}
