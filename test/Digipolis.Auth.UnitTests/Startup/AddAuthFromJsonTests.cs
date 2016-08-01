using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Digipolis.Auth.UnitTests.Startup
{
    public class AddAuthFromJsonTests : AddAuthBaseTests
    {
        public AddAuthFromJsonTests()
        {
            var basePath = $"{Directory.GetCurrentDirectory()}/_TestData";

            Act = services =>
            {
                services.AddAuth(options =>
                {
                    options.BasePath = basePath;
                    options.FileName = @"authconfig.json";
                    options.Section = "Auth";
                });
                services.AddOptions();
            };
        }
    }
}
