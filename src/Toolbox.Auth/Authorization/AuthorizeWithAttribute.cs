using Microsoft.AspNet.Authorization;

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
