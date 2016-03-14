namespace Toolbox.Auth.UnitTests.Startup
{
    public class AddAuthFromOptionsTests : AddAuthBaseTests
    {
        public AddAuthFromOptionsTests()
        {
            Act = services =>
            {
                services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.PdpUrl = "http://test.pdp.be/";
                    options.PdpCacheDuration = 60;
                    options.JwtAudience = "audience";
                    options.JwtIssuer = "issuer";
                    options.JwtSigningKeyProviderUrl = "singingKeyProviderUrl";
                    options.jwtSigningKeyProviderApikey = "singinKeyProviderApiKey";
                    options.JwtSigningKeyCacheDuration = 8;
                    options.JwtValidatorClockSkew = 3;
                });
            };
        }
    }
}
