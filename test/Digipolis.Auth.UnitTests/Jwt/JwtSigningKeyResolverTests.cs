using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.UnitTests.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class JwtSigningKeyResolverTests
    {
        private AuthOptions _options;
        private const string CACHE_KEY = "JwtSigningKey";
        private TestLogger<JwtSigningKeyResolver> _logger = TestLogger<JwtSigningKeyResolver>.CreateLogger();

        public JwtSigningKeyResolverTests()
        {
            _options = new AuthOptions { JwtSigningKeyCacheDuration = 0 };
        }

        [Fact]
        public void ThrowsExceptionIfCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyResolver(Mock.Of<HttpClient>(), null,
                Options.Create(new AuthOptions()),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyResolver(Mock.Of<HttpClient>(), Mock.Of<IMemoryCache>(), null,
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyResolver(Mock.Of<HttpClient>(), Mock.Of<IMemoryCache>(),
                Options.Create<AuthOptions>(null),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfHttpClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyResolver(null, Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
                _logger));
        }

        [Fact]
        public void ThrowsExceptionIfLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtSigningKeyResolver(Mock.Of<HttpClient>(), Mock.Of<IMemoryCache>(),
                Options.Create(new AuthOptions()),
                null));
        }

        [Fact]
        public void ThrowsExceptionWhenNoX5uHeaderPresent()
        {
            var mockedCache = CreateEmptyMockedCache();
            var resolver = new JwtSigningKeyResolver(Mock.Of<HttpClient>(), mockedCache.Object, Options.Create(_options), _logger);
            var token = "";
            SecurityToken securityToken = null;
            var tokenValidationParameters = new TokenValidationParameters();

            Assert.Throws<NullReferenceException>(() => resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters));
        }

        [Fact]
        public void X5uFromHeaderIsUsed()
        {
            var mockedCache = CreateEmptyMockedCache();
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, "");
            var client = new HttpClient(mockHandler);
            var resolver = new JwtSigningKeyResolver(client, mockedCache.Object, Options.Create(_options), _logger);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);
            var tokenValidationParameters = new TokenValidationParameters();

            try
            {
                var keys = resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters);
            }
            catch (Exception)
            {
            }

            Assert.Equal("http://localhost:5000/x5u", mockHandler.RequestUri.ToString());
        }

        [Fact]
        public void ResolveKey()
        {
            var mockedCache = CreateEmptyMockedCache();
            var cert = "{\"x5u\": \"LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQ0lqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FnOEFNSUlDQ2dLQ0FnRUFqbXJnN3NGeFJkb2JTWkhJMlpqawpucnBGVC9RclhEcFl6VVU4SU1hNFRPa2dFUnRaM09CbFpkbWNieXVmcEJuNTJmWDlYRWVIOVR1QjkxOWNQeEJFCnpKN0NzUmVTK1dxcHk5UFN3K3BtQ2lDZmpIZmx1ZDJ1dzUwbmVYOGVKeFl0ekhDN1VOOCt1QThvQ0tqdzBJM1AKK0VDYTdhVy9EbU1jSS81T3NyaXhlN2ZQenY4Q0V6aFRidzdBOTZuSzIrVkkvVXFGV2Yxb0Rzd2xYOFBPaHpMRQppdWo3eEJpdWJqbDZONERablF5YW84UzJFZ2ZQT05KNG1ySW42VEQwNzEvdE9NaDFHWUF3SnBWQ3YzYWdSUVdHCjhNaWxhYXlyQzRaNTNrNmRLV1FTNklmVTd3NWJnQjEraGdJenBoK05NbzdWWTROYkpYOTZ1b0Q3QW9pQjRvNjYKclMxakNLS3lEcUwwTTkwQzFIaDcrUit5TWhJa0ZkRUdDS0ZHaDNmbDlVREdKNEZEVG1vNGR1MENxbm13bWpvVgpmUmR0eW4rNjFBRHhQNnpkN24xTEFxeVBCNEVreHVrUTc3Sy9PTkxwUnYydHJmdDlvU1VSMWpXTXE3dzEyV1lyCmhZVnhPR1NvNU40RUdCSmpIQXlRZ01DUzhQWGdtN045WGFOdWtlczBZQ3lBTDlYU0JDRTNuNFQ0Qk9XRzlEMkIKQ0QvelV2bjVDRnl3Smh1ZzlSdzRMV3QwbzJHYXlpTjN5SDBwZFhBc2pTRlRiN1ZpdnBPc1cwL3k2aUdmMEJqSwpUOHlYSkVvOG9QcDRIMkl1TDR4TDQ4bW50Qm5WalBzSXRuemlHQ2pxZ0hCN2xxYjdxeXUvNit4dEhnTGxGb2MyCjBLQnZTYURGWWJiRXRPNE5GVnJNdUlFQ0F3RUFBUT09Ci0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==\"}";
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, cert);
            var client = new HttpClient(mockHandler);
            var resolver = new JwtSigningKeyResolver(client, mockedCache.Object, Options.Create(_options), _logger);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);
            var tokenValidationParameters = new TokenValidationParameters();

            var keys = resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters);

            Assert.NotNull(keys);
            Assert.NotEmpty(keys);
            Assert.IsType<RsaSecurityKey>(keys.First());

            var key = keys.First() as RsaSecurityKey;
            Assert.Equal(66571, key.Parameters.Modulus.Sum(b => b));
            Assert.Equal(2, key.Parameters.Exponent.Sum(b => b));
        }

        [Fact]
        public void CachedKeyIsReturned()
        {
            _options.JwtSigningKeyCacheDuration = 5;
            string key = "key";

            var mockedCache = CreateMockedCache(CACHE_KEY, key);
            var cert = "{\"x5u\": \"LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQ0lqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FnOEFNSUlDQ2dLQ0FnRUFqbXJnN3NGeFJkb2JTWkhJMlpqawpucnBGVC9RclhEcFl6VVU4SU1hNFRPa2dFUnRaM09CbFpkbWNieXVmcEJuNTJmWDlYRWVIOVR1QjkxOWNQeEJFCnpKN0NzUmVTK1dxcHk5UFN3K3BtQ2lDZmpIZmx1ZDJ1dzUwbmVYOGVKeFl0ekhDN1VOOCt1QThvQ0tqdzBJM1AKK0VDYTdhVy9EbU1jSS81T3NyaXhlN2ZQenY4Q0V6aFRidzdBOTZuSzIrVkkvVXFGV2Yxb0Rzd2xYOFBPaHpMRQppdWo3eEJpdWJqbDZONERablF5YW84UzJFZ2ZQT05KNG1ySW42VEQwNzEvdE9NaDFHWUF3SnBWQ3YzYWdSUVdHCjhNaWxhYXlyQzRaNTNrNmRLV1FTNklmVTd3NWJnQjEraGdJenBoK05NbzdWWTROYkpYOTZ1b0Q3QW9pQjRvNjYKclMxakNLS3lEcUwwTTkwQzFIaDcrUit5TWhJa0ZkRUdDS0ZHaDNmbDlVREdKNEZEVG1vNGR1MENxbm13bWpvVgpmUmR0eW4rNjFBRHhQNnpkN24xTEFxeVBCNEVreHVrUTc3Sy9PTkxwUnYydHJmdDlvU1VSMWpXTXE3dzEyV1lyCmhZVnhPR1NvNU40RUdCSmpIQXlRZ01DUzhQWGdtN045WGFOdWtlczBZQ3lBTDlYU0JDRTNuNFQ0Qk9XRzlEMkIKQ0QvelV2bjVDRnl3Smh1ZzlSdzRMV3QwbzJHYXlpTjN5SDBwZFhBc2pTRlRiN1ZpdnBPc1cwL3k2aUdmMEJqSwpUOHlYSkVvOG9QcDRIMkl1TDR4TDQ4bW50Qm5WalBzSXRuemlHQ2pxZ0hCN2xxYjdxeXUvNit4dEhnTGxGb2MyCjBLQnZTYURGWWJiRXRPNE5GVnJNdUlFQ0F3RUFBUT09Ci0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==\"}";
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, cert);
            var client = new HttpClient(mockHandler);
            var resolver = new JwtSigningKeyResolver(client, mockedCache.Object, Options.Create(_options), _logger);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);
            var tokenValidationParameters = new TokenValidationParameters();

            var keys = resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters);

            var securityKey = keys.First() as SymmetricSecurityKey;
            Assert.NotNull(securityKey);
            Assert.Equal(Encoding.UTF8.GetBytes(key), securityKey.Key);
        }

        [Fact]
        public void SetResponseToCache()
        {
            _options.JwtSigningKeyCacheDuration = 5;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry("JwtSigningKey"))
                .Returns(cacheEntry);

            var cert = "{\"x5u\": \"LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQ0lqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FnOEFNSUlDQ2dLQ0FnRUFqbXJnN3NGeFJkb2JTWkhJMlpqawpucnBGVC9RclhEcFl6VVU4SU1hNFRPa2dFUnRaM09CbFpkbWNieXVmcEJuNTJmWDlYRWVIOVR1QjkxOWNQeEJFCnpKN0NzUmVTK1dxcHk5UFN3K3BtQ2lDZmpIZmx1ZDJ1dzUwbmVYOGVKeFl0ekhDN1VOOCt1QThvQ0tqdzBJM1AKK0VDYTdhVy9EbU1jSS81T3NyaXhlN2ZQenY4Q0V6aFRidzdBOTZuSzIrVkkvVXFGV2Yxb0Rzd2xYOFBPaHpMRQppdWo3eEJpdWJqbDZONERablF5YW84UzJFZ2ZQT05KNG1ySW42VEQwNzEvdE9NaDFHWUF3SnBWQ3YzYWdSUVdHCjhNaWxhYXlyQzRaNTNrNmRLV1FTNklmVTd3NWJnQjEraGdJenBoK05NbzdWWTROYkpYOTZ1b0Q3QW9pQjRvNjYKclMxakNLS3lEcUwwTTkwQzFIaDcrUit5TWhJa0ZkRUdDS0ZHaDNmbDlVREdKNEZEVG1vNGR1MENxbm13bWpvVgpmUmR0eW4rNjFBRHhQNnpkN24xTEFxeVBCNEVreHVrUTc3Sy9PTkxwUnYydHJmdDlvU1VSMWpXTXE3dzEyV1lyCmhZVnhPR1NvNU40RUdCSmpIQXlRZ01DUzhQWGdtN045WGFOdWtlczBZQ3lBTDlYU0JDRTNuNFQ0Qk9XRzlEMkIKQ0QvelV2bjVDRnl3Smh1ZzlSdzRMV3QwbzJHYXlpTjN5SDBwZFhBc2pTRlRiN1ZpdnBPc1cwL3k2aUdmMEJqSwpUOHlYSkVvOG9QcDRIMkl1TDR4TDQ4bW50Qm5WalBzSXRuemlHQ2pxZ0hCN2xxYjdxeXUvNit4dEhnTGxGb2MyCjBLQnZTYURGWWJiRXRPNE5GVnJNdUlFQ0F3RUFBUT09Ci0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==\"}";
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, cert);
            var client = new HttpClient(mockHandler);
            var resolver = new JwtSigningKeyResolver(client, mockedCache.Object, Options.Create(_options), _logger);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);
            var tokenValidationParameters = new TokenValidationParameters();

            var keys = resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters);

            var cackedkey = cacheEntry.Value as RsaSecurityKey;

            Assert.Equal(66571, cackedkey.Parameters.Modulus.Sum(b => b));
            Assert.Equal(2, cackedkey.Parameters.Exponent.Sum(b => b));
        }

        [Fact]
        public void CacheDurationFromOptionsIsUsed()
        {
            _options.JwtSigningKeyCacheDuration = 5;
            var cacheEntry = new TestCacheEntry();
            var mockedCache = CreateEmptyMockedCache();
            mockedCache.Setup(c => c.CreateEntry("JwtSigningKey"))
                .Returns(cacheEntry);

            var cert = "{\"x5u\": \"LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQ0lqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FnOEFNSUlDQ2dLQ0FnRUFqbXJnN3NGeFJkb2JTWkhJMlpqawpucnBGVC9RclhEcFl6VVU4SU1hNFRPa2dFUnRaM09CbFpkbWNieXVmcEJuNTJmWDlYRWVIOVR1QjkxOWNQeEJFCnpKN0NzUmVTK1dxcHk5UFN3K3BtQ2lDZmpIZmx1ZDJ1dzUwbmVYOGVKeFl0ekhDN1VOOCt1QThvQ0tqdzBJM1AKK0VDYTdhVy9EbU1jSS81T3NyaXhlN2ZQenY4Q0V6aFRidzdBOTZuSzIrVkkvVXFGV2Yxb0Rzd2xYOFBPaHpMRQppdWo3eEJpdWJqbDZONERablF5YW84UzJFZ2ZQT05KNG1ySW42VEQwNzEvdE9NaDFHWUF3SnBWQ3YzYWdSUVdHCjhNaWxhYXlyQzRaNTNrNmRLV1FTNklmVTd3NWJnQjEraGdJenBoK05NbzdWWTROYkpYOTZ1b0Q3QW9pQjRvNjYKclMxakNLS3lEcUwwTTkwQzFIaDcrUit5TWhJa0ZkRUdDS0ZHaDNmbDlVREdKNEZEVG1vNGR1MENxbm13bWpvVgpmUmR0eW4rNjFBRHhQNnpkN24xTEFxeVBCNEVreHVrUTc3Sy9PTkxwUnYydHJmdDlvU1VSMWpXTXE3dzEyV1lyCmhZVnhPR1NvNU40RUdCSmpIQXlRZ01DUzhQWGdtN045WGFOdWtlczBZQ3lBTDlYU0JDRTNuNFQ0Qk9XRzlEMkIKQ0QvelV2bjVDRnl3Smh1ZzlSdzRMV3QwbzJHYXlpTjN5SDBwZFhBc2pTRlRiN1ZpdnBPc1cwL3k2aUdmMEJqSwpUOHlYSkVvOG9QcDRIMkl1TDR4TDQ4bW50Qm5WalBzSXRuemlHQ2pxZ0hCN2xxYjdxeXUvNit4dEhnTGxGb2MyCjBLQnZTYURGWWJiRXRPNE5GVnJNdUlFQ0F3RUFBUT09Ci0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==\"}";
            var mockHandler = new MockMessageHandler<string>(HttpStatusCode.OK, cert);
            var client = new HttpClient(mockHandler);
            var resolver = new JwtSigningKeyResolver(client, mockedCache.Object, Options.Create(_options), _logger);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsIng1dSI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMC94NXUifQ.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzI1NDk1NDgsImV4cCI6MTUwNDA4NTU0OCwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.jKg9l0cuTapEFcx9v1pLtBiigK_7EXlCqvKZBoS24XE";
            SecurityToken securityToken = new JwtSecurityToken(token);
            var tokenValidationParameters = new TokenValidationParameters();

            var keys = resolver.IssuerSigningKeyResolver(token, securityToken, "", tokenValidationParameters);

            var cackedkey = cacheEntry.Value as RsaSecurityKey;

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
