using Microsoft.AspNet.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolbox.Auth.Authorization
{
    public class AuthorizeWithAttribute : AuthorizeAttribute
    {
        public AuthorizeWithAttribute()
            :base (Policies.CustomBased)
        {
        }

        public string CustomPermission { get; set; }
        public string[] CustomPermissions { get; set; }
    }
}
