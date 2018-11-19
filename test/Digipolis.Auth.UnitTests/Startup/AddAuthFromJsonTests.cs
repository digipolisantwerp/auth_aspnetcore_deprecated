using Digipolis.ApplicationServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
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
                var mockHostingEnvironment = new Mock<IHostingEnvironment>();
                mockHostingEnvironment.Setup(h => h.EnvironmentName)
                    .Returns("IntegrationTesting");

                services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
                services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

                services.AddApplicationServices(setup =>
                {
                    setup.ApplicationId = Guid.NewGuid().ToString();
                });

                services.AddAuth(options =>
                {
                    options.BasePath = basePath;
                    options.FileName = @"authconfig.json";
                });
                services.AddOptions();
            };
        }
    }
}
