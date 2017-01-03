using Microsoft.IdentityModel.Tokens;

namespace Digipolis.Auth.Jwt
{
    public interface ITokenValidationParametersFactory
    {
        TokenValidationParameters Create();
    }
}
