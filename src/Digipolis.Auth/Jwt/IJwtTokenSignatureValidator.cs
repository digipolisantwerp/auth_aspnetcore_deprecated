using Microsoft.IdentityModel.Tokens;

namespace Digipolis.Auth.Jwt
{
    public interface IJwtTokenSignatureValidator
    {
        SecurityToken SignatureValidator(string token, TokenValidationParameters validationParameters);
    }
}