using Microsoft.AspNetCore.Authorization;

namespace Toolbox.Auth.Authorization
{
    public class AuthorizeWithAttribute : AuthorizeAttribute
    {
        public AuthorizeWithAttribute()
            :base (Policies.CustomBased)
        {
        }

        public string Permission { get; set; }
        public string[] Permissions { get; set; }
    }
}
