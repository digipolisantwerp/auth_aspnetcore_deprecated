using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public abstract class AddAuthDevPermissionsTests
    {
        public Action<ServiceCollection> Act { get; set; }

        [Fact]
        public void DevPolicyDecisionProviderIsRegisteredAsSingleton()
        {
            var services = new ServiceCollection();

            Act(services);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IPolicyDecisionProvider) &&
                                                     sd.ImplementationType == typeof(DevPolicyDecisionProvider))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        public void DevPermissionsOptionsAreRegisteredAsSingleton()
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
        public void DevPolicyDecisionProviderIsRegistred()
        {
            var services = new ServiceCollection();

            Act(services);

            var pdpProvider = services.BuildServiceProvider().GetService<IPolicyDecisionProvider>();

            Assert.IsType(typeof(DevPolicyDecisionProvider), pdpProvider);
        }
    }
}
