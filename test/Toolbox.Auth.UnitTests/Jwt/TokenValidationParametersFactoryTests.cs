using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class TokenValidationParametersFactoryTests
    {
        [Fact]
        public void SetValidateSignatureWhenJwtSigningKeyProviderUrlProvided()
        {
            var authOptions = new AuthOptions { JwtSigningKeyProviderUrl = "singingKeyUrl" };
            var signinKeyValidator = Mock.Of<IJwtTokenSignatureValidator>();

            var parameters = TokenValidationParametersFactory.Create(authOptions, signinKeyValidator);

            Assert.True(parameters.ValidateSignature);
        }

        [Fact]
        public void SetValidateSignatureToFalseWhenJwtSigningKeyProviderUrlNull()
        {
            var authOptions = new AuthOptions();
            var signinKeyValidator = Mock.Of<IJwtTokenSignatureValidator>();

            var parameters = TokenValidationParametersFactory.Create(authOptions, signinKeyValidator);

            Assert.False(parameters.ValidateSignature);
        }
    }
}
