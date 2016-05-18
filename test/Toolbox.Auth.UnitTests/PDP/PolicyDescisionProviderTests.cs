using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using Toolbox.Auth.UnitTests.Utilities;
using Xunit;

namespace Toolbox.Auth.UnitTests.PDP
{
    public class PolicyDescisionProviderTests
    {
        private string _application = "APP";
        private string _userId = "user123";
        private string _pdpUrl = "http://test.com";
        private string requestedresource = "requestedResource";
        private AuthOptions _options;
        private TestLogger<PolicyDescisionProvider> _logger = TestLogger<PolicyDescisionProvider>.CreateLogger();

        public PolicyDescisionProviderTests()
        {
            _options = new AuthOptions { PdpUrl = _pdpUrl, PdpCacheDuration = 0 };
        }

        [Fact]
        public void ThrowsExceptionIfCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDescisionProvider(null,
                Options.Create(new AuthOptions()),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDescisionProvider(Mock.Of<IMemoryCache>(), null,
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDescisionProvider(Mock.Of<IMemoryCache>(),
                Options.Create<AuthOptions>(null),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDescisionProvider(Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
               null,
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDescisionProvider(Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
                Mock.Of<HttpClientHandler>(),
                null));
        }

        [Fact]
        public async Task GetResponse()
        {
            var mockedCache = CreateEmptyMockedCache();

            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockHandler =new MockMessageHandler<PdpResponse>(HttpStatusCode.OK, pdpResponse);
            var provider = new PolicyDescisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(pdpResponse.applicationId, result.applicationId);
            Assert.Equal(pdpResponse.userId, result.userId);
            Assert.Equal(pdpResponse.permissions, result.permissions);
        }

        [Fact]
        public async Task ReturnsNullIfUserUnknown()
        {
            var mockedCache = CreateEmptyMockedCache();

            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.NotFound, null);
            var provider = new PolicyDescisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var result = await provider.GetPermissionsAsync("otherUser", _application);

            Assert.Null(result);
            Assert.NotEmpty(_logger.LoggedMessages);
        }

        [Fact]
        public async Task GetCachedPdpResponse()
        {
            _options.PdpCacheDuration = 60;
            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockedCache = CreateMockedCache(BuildCacheKey(_userId), pdpResponse);
            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.NotFound, null);
            var provider = new PolicyDescisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(pdpResponse, result);
        }

        [Fact]
        public async Task SetResponseToCache()
        {
            _options.PdpCacheDuration = 60;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry(BuildCacheKey(_userId)))
                .Returns(cacheEntry);

            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.OK, pdpResponse);
            var provider = new PolicyDescisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(pdpResponse.applicationId, ((PdpResponse)cacheEntry.Value).applicationId);
            Assert.Equal(pdpResponse.userId, ((PdpResponse)cacheEntry.Value).userId);
            Assert.Equal(pdpResponse.permissions, ((PdpResponse)cacheEntry.Value).permissions);
        }

        [Fact]
        public async Task CacheDurationFromOptionsIsUsed()
        {
            _options.PdpCacheDuration = 60;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry(BuildCacheKey(_userId)))
                .Returns(cacheEntry);

            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.OK, pdpResponse);
            var provider = new PolicyDescisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.True(cacheEntry.AbsoluteExpirationRelativeToNow.Value == new TimeSpan(0, _options.PdpCacheDuration, 0));
        }

        private string BuildCacheKey(string userId) => $"pdpResponse-{userId}";

        private Mock<IMemoryCache> CreateEmptyMockedCache()
        {
            return CreateMockedCache("", null);
        }

        private Mock<IMemoryCache> CreateMockedCache(string key, PdpResponse pdpResponse)
        {
            var mockCache = new Mock<IMemoryCache>();
            var cachedObject = pdpResponse as object;

            mockCache.Setup(c => c.TryGetValue("", out cachedObject))
                .Returns(false);

            mockCache.Setup(c => c.TryGetValue(key, out cachedObject))
                .Returns(true);

            return mockCache;
        }
    }
}
