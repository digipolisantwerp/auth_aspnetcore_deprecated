using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Auth.Jwt
{
    public class JwtTokenSignatureValidator : IJwtTokenSignatureValidator
    {
        private readonly IJwtSigningKeyProvider _jwtSigningKeyProvider;

        public JwtTokenSignatureValidator(IJwtSigningKeyProvider jwtSigningKeyProvider)
        {
            if (jwtSigningKeyProvider == null) throw new ArgumentNullException(nameof(jwtSigningKeyProvider), $"{nameof(jwtSigningKeyProvider)} cannot be null");

            _jwtSigningKeyProvider = jwtSigningKeyProvider;
        }

        public SecurityToken SignatureValidator(string token, TokenValidationParameters validationParameters)
        {
            var jwt = new JwtSecurityToken(token);

            var isValid = ValidateSignature(jwt, validationParameters.IssuerSigningKey);

            if (isValid == false)
            {
                validationParameters.IssuerSigningKey = _jwtSigningKeyProvider.ResolveSigningKeyAsync(false).Result;

                isValid = ValidateSignature(jwt, validationParameters.IssuerSigningKey);

                if (isValid == false)
                    throw new Exception("Invalid Jwt signature.");
            }
            return jwt;
        }

        public bool ValidateSignature(JwtSecurityToken token, SecurityKey securityKey)
        {
            var encodedData = token.RawHeader + "." + token.RawPayload;
            var key = (securityKey as SymmetricSecurityKey).Key;
            var signature = CreateSignature(encodedData, key);

            if (signature == token.RawSignature)
                return true;

            return false;
        }

        private static string CreateSignature(string input, byte[] key)
        {
            HMACSHA256 hmacsha = new HMACSHA256(key);
            byte[] byteArray = Encoding.UTF8.GetBytes(input);
            byte[] hashValue = hmacsha.ComputeHash(byteArray);
            return Base64UrlEncoder.Encode(hashValue);
        }
    }
}
