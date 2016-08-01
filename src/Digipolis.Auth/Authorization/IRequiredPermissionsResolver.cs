using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Digipolis.Auth.Authorization
{
    public interface IRequiredPermissionsResolver
    {
        IEnumerable<string> ResolveFromAttributeProperties(AuthorizationHandlerContext context);
        string ResolveFromConvention(AuthorizationHandlerContext context);
    }
}