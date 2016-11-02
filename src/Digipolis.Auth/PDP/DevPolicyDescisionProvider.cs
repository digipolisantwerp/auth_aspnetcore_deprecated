using Digipolis.Auth.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public class DevPolicyDescisionProvider : IPolicyDescisionProvider
    {
        private readonly DevPermissionsOptions _permissions;

        public DevPolicyDescisionProvider(IOptions<DevPermissionsOptions> options)
        {
            if (options == null || options.Value == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");

            _permissions = options.Value;
        }

        public Task<PdpResponse> GetPermissionsAsync(string user, string application)
        {
            var pdpResponse = new PdpResponse
            {
                applicationId = application,
                userId = user,
                permissions = _permissions.Permissions
            };

            return Task.FromResult<PdpResponse>(pdpResponse);
        }
    }
}
