using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.UnitTests.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.PDP
{
    public class PolicyDecisionProviderTests
    {
        private string _application = "APP";
        private string _userId = "user123";
        private string _pdpUrl = "http://test.com";
        private string requestedresource = "requestedResource";
        private string _apiKey = "apiKeyValue";
        private AuthOptions _options;
        private TestLogger<PolicyDecisionProvider> _logger = TestLogger<PolicyDecisionProvider>.CreateLogger();

        public PolicyDecisionProviderTests()
        {
            _options = new AuthOptions { PdpUrl = _pdpUrl, PdpCacheDuration = 0, PdpApiKey = _apiKey };
        }

        [Fact]
        public void ThrowsExceptionIfCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDecisionProvider(null,
                Options.Create(new AuthOptions()),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDecisionProvider(Mock.Of<IMemoryCache>(), null,
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDecisionProvider(Mock.Of<IMemoryCache>(),
                Options.Create<AuthOptions>(null),
                Mock.Of<HttpClientHandler>(),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDecisionProvider(Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
               null,
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PolicyDecisionProvider(Mock.Of<IMemoryCache>(),
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
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(pdpResponse.applicationId, result.applicationId);
            Assert.Equal(pdpResponse.userId, result.userId);
            Assert.Equal(pdpResponse.permissions, result.permissions);
        }

        [Fact]
        public async Task ApiKeyIsSetInHeader()
        {
            var mockedCache = CreateEmptyMockedCache();

            var pdpResponse = new PdpResponse
            {
                applicationId = _application,
                userId = _userId,
                permissions = new List<String>(new string[] { requestedresource })
            };

            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.OK, pdpResponse);
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(_apiKey, mockHandler.Headers.GetValues(HeaderKeys.Apikey).FirstOrDefault());
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
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);
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
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

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
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            var result = await provider.GetPermissionsAsync(_userId, _application);

            Assert.Equal(pdpResponse.applicationId, ((PdpResponse)cacheEntry.Value).applicationId);
            Assert.Equal(pdpResponse.userId, ((PdpResponse)cacheEntry.Value).userId);
            Assert.Equal(pdpResponse.permissions, ((PdpResponse)cacheEntry.Value).permissions);
        }

        [Fact]
        public async Task ShouldNotCacheResponseWithoutPermissions()
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
                permissions = new List<String>(new string[] { })
            };

            var mockHandler = new MockMessageHandler<PdpResponse>(HttpStatusCode.OK, pdpResponse);
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

            var result = await provider.GetPermissionsAsync(_userId, _application);

            mockedCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Never);
            Assert.Null(cacheEntry.Value);
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
            var provider = new PolicyDecisionProvider(mockedCache.Object, Options.Create(_options), mockHandler, _logger);

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
