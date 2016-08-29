using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Text;
using Digipolis.Auth.Jwt;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    //public class JwtTokenSignatureValidatorTests
    //{
    //    [Fact]
    //    public void ValidateHS256Token()
    //    {
    //        var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NTcwMjA0NzAsImV4cCI6MTQ4ODU1NjczNywiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSJ9.ihr1jlf4-MUuB6PzmhPYZpklv27SSGyvoDX3jyOxu1M";

    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret"));

    //        var validator = new JwtTokenSignatureValidator(CreateSigningKeyProvider(key));

    //        var parameters = new TokenValidationParameters
    //        {
    //            IssuerSigningKey = key
    //        };

    //        var jwt = validator.SignatureValidator(token, parameters);

    //        Assert.NotNull(jwt);
    //    }


    //    [Fact]
    //    public void ThrowsExceptionWhenNonHS256TokenInvalid()
    //    {
    //        var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJpc3MiOiJPbmxpbmUgSldUIEJ1aWxkZXIiLCJpYXQiOjE0NzAzOTE5MjEsImV4cCI6MTUwMTkyNzkyMSwiYXVkIjoid3d3LmV4YW1wbGUuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG5ueSIsIlN1cm5hbWUiOiJSb2NrZXQiLCJFbWFpbCI6Impyb2NrZXRAZXhhbXBsZS5jb20iLCJSb2xlIjpbIk1hbmFnZXIiLCJQcm9qZWN0IEFkbWluaXN0cmF0b3IiXX0.-hU8hHqwEUuG52Ebw4z5QiAFRUGT8kckVGKytrrz_f4Ra96Rxirk2vTyLbqYZbC3wDF7gFwR47su4yK3iC3GTA";

    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("invalidsecret"));

    //        var validator = new JwtTokenSignatureValidator(CreateSigningKeyProvider(key));

    //        var parameters = new TokenValidationParameters
    //        {
    //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("invalidsecret"))
    //        };

    //        var ex = Assert.Throws<Exception>(() => validator.SignatureValidator(token, parameters));
    //    }

    //    private IJwtSigningCertificateProvider CreateSigningKeyProvider(SecurityKey key)
    //    {
    //        var mockProvider = new Mock<IJwtSigningCertificateProvider>();

    //        mockProvider.Setup(p => p.ResolveSigningKeyAsync(false))
    //            .ReturnsAsync(key);

    //        return mockProvider.Object;
    //    }
    //}
}
