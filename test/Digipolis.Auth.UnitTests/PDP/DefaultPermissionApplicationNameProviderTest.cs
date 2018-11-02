using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using System;
using Xunit;

namespace Digipolis.Auth.UnitTests.PDP
{
    public class DefaultPermissionApplicationNameProviderTest
    {
        [Fact]
        public void ThrowsExceptionIfOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultPermissionApplicationNameProvider(null));
        }
        
        [Fact]
        public void ThrowsExceptionIfOptionsValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultPermissionApplicationNameProvider(Options.Create<AuthOptions>(null)));
        }

        [Fact]
        public void ApplicationNameReturnsTheApplicationNameFromAuthOptions()
        {
            var applicationName = "SOMECOOLAPP";
            var authOptions = new AuthOptions()
            {
                ApplicationName = applicationName
            };
            var options = Options.Create(authOptions);
            
            var subject = new DefaultPermissionApplicationNameProvider(options);
            
            Assert.Equal(applicationName, subject.ApplicationName(new System.Security.Claims.ClaimsPrincipal()));
        }
    }
}