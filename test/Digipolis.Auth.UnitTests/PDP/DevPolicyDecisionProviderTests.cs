using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.PDP
{
    public class DevDevPolicyDecisionProviderTests
    {
        private string _application = "APP";
        private string _userId1 = "user1";
        private string[] _permissions = { "requestedResource1" };
        private DevPermissionsOptions _options;

        public DevDevPolicyDecisionProviderTests()
        {
            _options = new DevPermissionsOptions
            {
                //Permissions = new List<PdpResponse>()
                //{
                //    new PdpResponse {  applicationId = _application, userId = _userId1, permissions = _permissions1 },
                //    new PdpResponse {  applicationId = _application, userId = _userId2, permissions = _permissions2 }
                //}
                Permissions = new List<string>(_permissions)
            };
        }

       [Fact]
        public void ThrowsExceptionIfOptionsWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DevPolicyDecisionProvider(null));
        }

        
        [Fact]
        public async Task GetResponse()
        {
            var provider = new DevPolicyDecisionProvider(Options.Create(_options));

            var result = await provider.GetPermissionsAsync(_userId1, _application);

            Assert.Equal(_application, result.applicationId);
            Assert.Equal(_userId1, result.userId);
            Assert.Collection(result.permissions, x => Assert.Equal(_permissions[0], x));
        }
    }
}
