using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public class AddAuthDevPermissionsFromJsonTests : AddAuthDevPermissionsTests
    {
        public AddAuthDevPermissionsFromJsonTests()
        {
            var basePath = $"{Directory.GetCurrentDirectory()}/_TestData";

            Act = services =>
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>();
                mockHostingEnvironment.Setup(h => h.EnvironmentName)
                    .Returns("Development");

                services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

                services.AddAuth(options =>
                {
                    options.BasePath = basePath;
                    options.FileName = @"authconfig2.json";
                });
                services.AddOptions();
            };
        }
    }
}
