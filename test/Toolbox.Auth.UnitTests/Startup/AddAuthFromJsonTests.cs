using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Auth.UnitTests.Startup
{
    public class AddAuthFromJsonTests : AddAuthBaseTests
    {
        public AddAuthFromJsonTests()
        {
            Act = services =>
            {
                services.AddAuth(options =>
                {
                    options.FileName = @"_TestData/authconfig.json";
                    options.Section = "Auth";
                });
                services.AddOptions();
            };
        }
    }
}
