using Digipolis.Auth.Authorization;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public abstract class AddAuthDevPermissionsTests
    {
        public Action<ServiceCollection> Act { get; set; }

        [Fact]
        public void DevPolicyDescisionProviderIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IPolicyDescisionProvider) &&
                                                     sd.ImplementationType == typeof(DevPolicyDescisionProvider))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void DevPermissionsOptionsAreRegistratedAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<DevPermissionsOptions>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<DevPermissionsOptions>;
            Assert.NotNull(configOptions);

            var devPermissionsOptions = new DevPermissionsOptions();
            configOptions.Configure(devPermissionsOptions);

            Assert.Equal(true, devPermissionsOptions.UseDevPermissions);
            Assert.NotEmpty(devPermissionsOptions.Permissions);
        }

        [Fact]
        public void DevPolicyDescisionProviderIsRegistred()
        {
            var services = new ServiceCollection();

            Act(services);

            var pdpProvider = services.BuildServiceProvider().GetService<IPolicyDescisionProvider>();

            Assert.IsType(typeof(DevPolicyDescisionProvider), pdpProvider);
        }
    }
}
