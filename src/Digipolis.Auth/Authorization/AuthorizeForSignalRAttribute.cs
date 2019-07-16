using Microsoft.AspNetCore.Authorization;

namespace Digipolis.Auth.Authorization
{
    public class AuthorizeForSignalRAttribute : AuthorizeAttribute
    {
        public AuthorizeForSignalRAttribute()
            : base(Policies.SignalRBased)
        {
        }
    }
}
