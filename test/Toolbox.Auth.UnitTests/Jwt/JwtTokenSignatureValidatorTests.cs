using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class JwtTokenSignatureValidatorTests
    {
        [Fact]
        public void ValidateToken()
        {
            //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJhdWQiOiJTYW1wbGVBcHAiLCJleHAiOjE0NTcwMDIxNTAsImp0aSI6ImhXYXhrMG1CalZUNHc1WHdLUlI3eHciLCJpYXQiOjE0NTY5OTg1NTAsIm5iZiI6MTQ1Njk5ODQzMCwic3ViIjoiZXgwMjU1MEBkaWdhbnQuYW50d2VycGVuLmxvY2FsIiwibmFtZSI6ImV4MDI1NTAiLCJzdXJuYW1lIjoiSGFubm9uIiwiZ2l2ZW5uYW1lIjoiSmltbXkifQ.n6tih6JQuATq0opfiPunb5rHc7nmWg23eQMMfg6310w";
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NTcwMjA0NzAsImV4cCI6MTQ4ODU1NjczNywiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSJ9.ihr1jlf4-MUuB6PzmhPYZpklv27SSGyvoDX3jyOxu1M";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret"));

            var validator = new JwtTokenSignatureValidator(CreateSigningKeyProvider(key));

            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = key
            };

            var jwt = validator.SignatureValidator(token, parameters);

            Assert.NotNull(jwt);
        }

        [Fact]
        public void ReValidateTokenWhenCachedSigningKeyInvalid()
        {
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJhdWQiOiJTYW1wbGVBcHAiLCJleHAiOjE0NTcwMDIxNTAsImp0aSI6ImhXYXhrMG1CalZUNHc1WHdLUlI3eHciLCJpYXQiOjE0NTY5OTg1NTAsIm5iZiI6MTQ1Njk5ODQzMCwic3ViIjoiZXgwMjU1MEBkaWdhbnQuYW50d2VycGVuLmxvY2FsIiwibmFtZSI6ImV4MDI1NTAiLCJzdXJuYW1lIjoiSGFubm9uIiwiZ2l2ZW5uYW1lIjoiSmltbXkifQ.n6tih6JQuATq0opfiPunb5rHc7nmWg23eQMMfg6310w";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret"));

            var validator = new JwtTokenSignatureValidator(CreateSigningKeyProvider(key));

            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("invalidsecret"))
            };

            var jwt = validator.SignatureValidator(token, parameters);

            Assert.NotNull(jwt);
        }

        [Fact]
        public void ThrowsExceptionWhenSigningKeysInvalid()
        {
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJhdWQiOiJTYW1wbGVBcHAiLCJleHAiOjE0NTcwMDIxNTAsImp0aSI6ImhXYXhrMG1CalZUNHc1WHdLUlI3eHciLCJpYXQiOjE0NTY5OTg1NTAsIm5iZiI6MTQ1Njk5ODQzMCwic3ViIjoiZXgwMjU1MEBkaWdhbnQuYW50d2VycGVuLmxvY2FsIiwibmFtZSI6ImV4MDI1NTAiLCJzdXJuYW1lIjoiSGFubm9uIiwiZ2l2ZW5uYW1lIjoiSmltbXkifQ.n6tih6JQuATq0opfiPunb5rHc7nmWg23eQMMfg6310w";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("invalidsecret"));

            var validator = new JwtTokenSignatureValidator(CreateSigningKeyProvider(key));

            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("invalidsecret"))
            };

            var ex = Assert.Throws<Exception>(() => validator.SignatureValidator(token, parameters));
        }

        private IJwtSigningKeyProvider CreateSigningKeyProvider(SecurityKey key)
        {
            var mockProvider = new Mock<IJwtSigningKeyProvider>();

            mockProvider.Setup(p => p.ResolveSigningKey(false))
                .ReturnsAsync(key);

            return mockProvider.Object;
        }
    }
}
