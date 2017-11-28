using Digipolis.Auth.PDP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.Auth.Options
{
    public class DevPermissionsOptions
    {
        public bool UseDevPermissions { get; set; }
        public bool RequireSignedTokens { get; set; } = true;
        public string Environment { get; set; } = "";
        public List<string> Permissions { get; set; }
        public bool ValidateTokenLifetime { get; set; } = true;
    }
}
