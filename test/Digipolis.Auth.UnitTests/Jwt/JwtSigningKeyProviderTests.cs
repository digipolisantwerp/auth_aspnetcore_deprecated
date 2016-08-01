using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.UnitTests.Utilities;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class JwtSigningKeyProviderTests
    {
        private string _jwtKeyProviderUrl = "http://test.com";
        private AuthOptions _options;
        private const string CACHE_KEY = "JwtSigningKey";
        private TestLogger<JwtSigningKeyProvider> _logger = TestLogger<JwtSigningKeyProvider>.CreateLogger();

        public JwtSigningKeyProviderTests()
        {
            _options = new AuthOptions { JwtSigningKeyProviderUrl = _jwtKeyProviderUrl, JwtSigningKeyCacheDuration = 0 };
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
            _options.JwtSigningKeyCacheDuration = 5;
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
            _options.JwtSigningKeyCacheDuration = 5;
            string nonCachedKey = "nonCachedKey";

            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry("JwtSigningKey"))
                .Returns(cacheEntry);

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, nonCachedKey);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var securityKey = await provider.ResolveSigningKeyAsync(false) as SymmetricSecurityKey;

            Assert.NotNull(securityKey);
            Assert.Equal(Encoding.UTF8.GetBytes(nonCachedKey), securityKey.Key);
        }

        [Fact]
        public async Task SetResponseToCache()
        {
            _options.JwtSigningKeyCacheDuration = 5;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry("JwtSigningKey"))
                .Returns(cacheEntry);

            string key = "signingkey";

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, key);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            await provider.ResolveSigningKeyAsync(false);

            Assert.Equal(key, Encoding.UTF8.GetString(((SymmetricSecurityKey)cacheEntry.Value).Key));
        }

        [Fact]
        public async Task CacheDurationFromOptionsIsUsed()
        {
            _options.JwtSigningKeyCacheDuration = 5;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry("JwtSigningKey"))
                .Returns(cacheEntry);

            string key = "signingkey";

            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, key);
            var provider = new JwtSigningKeyProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            await provider.ResolveSigningKeyAsync(false);

            Assert.True(cacheEntry.AbsoluteExpirationRelativeToNow.Value == new TimeSpan(0, _options.JwtSigningKeyCacheDuration, 0));
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
