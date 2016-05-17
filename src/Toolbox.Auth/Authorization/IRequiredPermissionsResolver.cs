using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Toolbox.Auth.Authorization
{
    public interface IRequiredPermissionsResolver
    {
        IEnumerable<string> ResolveFromAttributeProperties(AuthorizationContext context);
        string ResolveFromConvention(AuthorizationContext context);
    }
}