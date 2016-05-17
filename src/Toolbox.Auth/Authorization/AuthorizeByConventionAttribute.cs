using Microsoft.AspNetCore.Authorization;

namespace Toolbox.Auth.Authorization
{
    public class AuthorizeByConventionAttribute : AuthorizeAttribute
    {
        public AuthorizeByConventionAttribute()
            :base (Policies.ConventionBased)
        {
        }
    }
}
