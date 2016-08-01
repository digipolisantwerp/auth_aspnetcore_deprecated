using Microsoft.AspNetCore.Authorization;

namespace Digipolis.Auth.Authorization
{
    public class AuthorizeByConventionAttribute : AuthorizeAttribute
    {
        public AuthorizeByConventionAttribute()
            :base (Policies.ConventionBased)
        {
        }
    }
}
