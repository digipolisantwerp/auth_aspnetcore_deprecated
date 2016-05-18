using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class JwtSigningKeyProviderTests
    {
        private string _jwtKeyProviderUrl = "http://test.com";
        private AuthOptions _options;
        private const string CACHE_KEY = "JwtSigningKey";
        private TestLogger<JwtSigningKeyProvider> _logger = TestLogger<JwtSigningKeyProvider>.CreateLogger();

        public JwtSigningKeyProviderTests()
        {
            _options = new AuthOptions { JwtSigningKeyProviderUrl = _jwtKeyProviderUrl, JwtSigningKeyCacheDuration = 5 };
        }

        [Fact]
        public void ThrowsExceptionIfCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyProvider(null,
                Options.Create(new AuthOptions()),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyProvider(Mock.Of<IMemoryCache>(), null,
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyProvider(Mock.Of<IMemoryCache>(),
                Options.Create<AuthOptions>(null),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyProvider(Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
                null,
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyProvider(Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
                Mock.Of<HttpClientHandler>(),
                null));
        }

        [Fact]
        public async Task GetKey()
        {
            var mockedCache = CreateEmptyMockedCache();
            string key = "signingkey";

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, key);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var securityKey = await provider.ResolveSigningKeyAsync(true) as SymmetricSecurityKey;

            Assert.NotNull(securityKey);
            Assert.Equal(Encoding.UTF8.GetBytes(key), securityKey.Key);
        }

        [Fact]
        public async Task ReturnsNullWhenNotFound()
        {
            var mockedCache = CreateEmptyMockedCache();

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.NotFound, null);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var securityKey = await provider.ResolveSigningKeyAsync(true) as SymmetricSecurityKey;

            Assert.Null(securityKey);
            Assert.NotEmpty(_logger.LoggedMessages);
        }

        [Fact]
        public async Task GetCachedKey()
        {
            string key = "signingkey";

            var mockedCache = CreateMockedCache(CACHE_KEY, key);
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.NotFound, null);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var securityKey = await provider.ResolveSigningKeyAsync(true) as SymmetricSecurityKey;

            Assert.NotNull(securityKey);
            Assert.Equal(Encoding.UTF8.GetBytes(key), securityKey.Key);
        }

        [Fact]
        public async Task IgnoreCachedKey()
        {
            string cachedKey = "cachedkey";
            string nonCachedKey = "nonCachedKey";

            var mockedCache = CreateMockedCache(CACHE_KEY, cachedKey);
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, nonCachedKey);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var securityKey = await provider.ResolveSigningKeyAsync(false) as SymmetricSecurityKey;

            Assert.NotNull(securityKey);
            Assert.Equal(Encoding.UTF8.GetBytes(nonCachedKey), securityKey.Key);
        }

        [Fact]
        public async Task SetResponseToCache()
        {
            var mockedCache = CreateEmptyMockedCache();
            string key = "signingkey";

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, key);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            await provider.ResolveSigningKeyAsync(false);

            //Should set object twice to cache: once in the constructor when preloading the cache and once when the call is made
            mockedCache.Verify(m => m.Set((object)CACHE_KEY, It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CacheDurationFromOptionsIsUsed()
        {
            MemoryCacheEntryOptions memoryCacheEntryOptions = null;

            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()))
                .Callback<object, object, MemoryCacheEntryOptions>((a, b, options) => memoryCacheEntryOptions = options);

            string key = "signingkey";

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, key);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            await provider.ResolveSigningKeyAsync(false);

            Assert.True(memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow.Value == new TimeSpan(0, _options.JwtSigningKeyCacheDuration, 0));
        }
               

        private Mock<IMemoryCache> CreateEmptyMockedCache()
        {
            return CreateMockedCache("", null);
        }

        private Mock<IMemoryCache> CreateMockedCache(string cacheKey, string securityKey)
        {
            var mockCache = new Mock<IMemoryCache>();
            var cachedObject = securityKey != null ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)) : null as object;

            mockCache.Setup(c => c.TryGetValue("", out cachedObject))
                .Returns(false);

            mockCache.Setup(c => c.TryGetValue(cacheKey, out cachedObject))
                .Returns(true);

            return mockCache;
        }
    }
}
