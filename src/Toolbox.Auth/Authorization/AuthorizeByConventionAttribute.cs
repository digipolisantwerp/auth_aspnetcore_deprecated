using Microsoft.AspNet.Authorization;

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
