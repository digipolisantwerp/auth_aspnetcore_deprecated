using Digipolis.Auth.PDP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Digipolis.Auth.UnitTests.Startup
{
    public class AddAuthDevPermissionsFromOptionsTests : AddAuthDevPermissionsTests
    {
        public AddAuthDevPermissionsFromOptionsTests()
        {
            Act = services =>
            {
                var mockHostingEnvironment = new Mock<IHostingEnvironment>();
                mockHostingEnvironment.Setup(h => h.EnvironmentName)
                    .Returns("Development");

                services.AddSingleton<IHostingEnvironment>(mockHostingEnvironment.Object);

                services.AddAuth(options =>
                {
                }, devPermissionsOptions =>
                {
                    devPermissionsOptions.UseDevPermissions = true;
                    //devPermissionsOptions.Permissions = new List<PdpResponse>
                    //{
                    //            new PdpResponse { applicationId = "AppName", userId = "user1", permissions = new List<string> { "permission1" } }
                    //};
                    devPermissionsOptions.Permissions = new List<string> { "permission1" };
                });
                services.AddOptions();
            };
        }
    }
}
