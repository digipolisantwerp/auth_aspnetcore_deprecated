using System.IdentityModel.Tokens;

namespace Toolbox.Auth.Jwt
{
    public interface IJwtTokenSignatureValidator
    {
        SecurityToken SignatureValidator(string token, TokenValidationParameters validationParameters);
    }
}