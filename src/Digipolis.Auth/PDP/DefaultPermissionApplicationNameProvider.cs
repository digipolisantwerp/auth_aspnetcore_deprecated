using System;
using Digipolis.Auth.Options;
using Microsoft.Extensions.Options;

namespace Digipolis.Auth.PDP
{
    public class DefaultPermissionApplicationNameProvider : IPermissionApplicationNameProvider
    {
        private readonly AuthOptions _authOptions;

        public DefaultPermissionApplicationNameProvider(IOptions<AuthOptions> authOptions)
        {
            if( authOptions==null || authOptions.Value == null ) throw new ArgumentNullException(nameof(authOptions), $"{nameof(authOptions)} cannot be null");
            _authOptions = authOptions.Value;
        }
        
        public string ApplicationName()
        {
            return _authOptions.ApplicationName;
        }
    }
}