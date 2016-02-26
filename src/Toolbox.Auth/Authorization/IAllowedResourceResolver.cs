using Microsoft.AspNet.Authorization;
using System.Collections.Generic;

namespace Toolbox.Auth.Authorization
{
    public interface IAllowedResourceResolver
    {
        IEnumerable<string> ResolveFromAttributeProperties(AuthorizationContext context);
        string ResolveFromConvention(AuthorizationContext context);
    }
}