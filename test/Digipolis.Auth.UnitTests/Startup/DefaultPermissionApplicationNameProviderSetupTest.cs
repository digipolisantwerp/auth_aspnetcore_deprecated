using System.Linq;
using System.Security.Claims;
using Digipolis.ApplicationServices;
using Digipolis.Auth.PDP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Digipolis.Auth.UnitTests.Startup
{
    public class DefaultPermissionApplicationNameProviderSetupTest
    {
        [Fact]
        public void ByDefaultTheDefaultPermissionApplicationNameProviderMustBeRegistered()
        {
            var applicationName = "ANOTHERCOOLAPP";
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IApplicationContext>((serviceProvider) => { return Mock.Of<IApplicationContext>(); });
            serviceCollection.AddTransient<IHostingEnvironment>((serviceProvider) => { return Mock.Of<IHostingEnvironment>(); });
            serviceCollection.AddAuth(options =>
            {
                options.EnableCookieAuth = false;
                options.UseDotnetKeystore = false;
                options.EnableJwtHeaderAuth = false;
                options.ApplicationName = applicationName;
            });
            
            var registrations = serviceCollection.Where(sd => sd.ServiceType == typeof(IPermissionApplicationNameProvider) &&
                                                     sd.ImplementationType == typeof(DefaultPermissionApplicationNameProvider))
                .ToArray();

            Assert.Single(registrations);
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }
        
        [Fact]
        public void ItIsPossibleToOverrideTheIPermissionApplicationNameProviderImplementation()
        {
            var applicationName = "ANOTHERCOOLAPP";
            var overridenApplicationName = "OVERRIDEN";
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IApplicationContext>((services) => { return Mock.Of<IApplicationContext>(); });
            serviceCollection.AddTransient<IHostingEnvironment>((services) => { return Mock.Of<IHostingEnvironment>(); });
            serviceCollection.AddAuth(options =>
            {
                options.EnableCookieAuth = false;
                options.UseDotnetKeystore = false;
                options.EnableJwtHeaderAuth = false;
                options.ApplicationName = applicationName;
            });
            serviceCollection.AddScoped<IPermissionApplicationNameProvider>((services) =>
                new DummyPermissionApplicationNameProvider(overridenApplicationName));

            var serviceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;
            var appNameProvider = serviceProvider.GetRequiredService<IPermissionApplicationNameProvider>();
            
            Assert.Equal(overridenApplicationName, appNameProvider.ApplicationName(new System.Security.Claims.ClaimsPrincipal()));
        }
        
        [Fact]
        public void ItIsPossibleToOverrideTheIPermissionApplicationNameProviderImplementationBeforeCallingAddAuth()
        {
            var applicationName = "ANOTHERCOOLAPP";
            var overridenApplicationName = "OVERRIDEN";
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IPermissionApplicationNameProvider>((services) =>
                new DummyPermissionApplicationNameProvider(overridenApplicationName));
            serviceCollection.AddTransient<IApplicationContext>((services) => { return Mock.Of<IApplicationContext>(); });
            serviceCollection.AddTransient<IHostingEnvironment>((services) => { return Mock.Of<IHostingEnvironment>(); });
            serviceCollection.AddAuth(options =>
            {
                options.EnableCookieAuth = false;
                options.UseDotnetKeystore = false;
                options.EnableJwtHeaderAuth = false;
                options.ApplicationName = applicationName;
            });

            var serviceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;
            var appNameProvider = serviceProvider.GetRequiredService<IPermissionApplicationNameProvider>();
            
            Assert.Equal(overridenApplicationName, appNameProvider.ApplicationName(new System.Security.Claims.ClaimsPrincipal()));
        }
    }

    class DummyPermissionApplicationNameProvider : IPermissionApplicationNameProvider
    {
        private readonly string _applicationName;

        public DummyPermissionApplicationNameProvider(string applicationName)
        {
            _applicationName = applicationName;
        }
        
        public string ApplicationName(ClaimsPrincipal principal)
        {
            return _applicationName;
        }
    }
}