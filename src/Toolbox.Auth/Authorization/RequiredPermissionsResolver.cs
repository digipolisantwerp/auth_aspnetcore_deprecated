using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc.Controllers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using static Toolbox.Auth.HttpMethods;
using static Toolbox.Auth.Operations;

namespace Toolbox.Auth.Authorization
{
    public class RequiredPermissionsResolver : IRequiredPermissionsResolver
    {
        public IEnumerable<string> ResolveFromAttributeProperties(AuthorizationContext context)
        {
            var authContext = context.Resource as Microsoft.AspNet.Mvc.Filters.AuthorizationContext;
            var actionDescriptor = authContext.ActionDescriptor as ControllerActionDescriptor;

            var requiredPermissions = new List<string>();
            var authorizePermissionsAttribute = actionDescriptor.MethodInfo.CustomAttributes
                                                                .FirstOrDefault(a => a.AttributeType == typeof(AuthorizeWithAttribute));

            if (authorizePermissionsAttribute != null)
            {
                var permissions = authorizePermissionsAttribute.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.Permissions))
                    .TypedValue.Value as ReadOnlyCollection<CustomAttributeTypedArgument>;

                if (permissions != null)
                    permissions.ToList<CustomAttributeTypedArgument>().ForEach(p => requiredPermissions.Add(p.Value.ToString()));

                var permission = authorizePermissionsAttribute.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.Permission))
                    .TypedValue.Value?.ToString();

                if (permission != null)
                    requiredPermissions.Add(permission);
            }

            return requiredPermissions;
        }

        public string ResolveFromConvention(AuthorizationContext context)
        {
            var authContext = context.Resource as Microsoft.AspNet.Mvc.Filters.AuthorizationContext;
            var actionDescriptor = authContext.ActionDescriptor as ControllerActionDescriptor;

            var actionPart = "";
            var httpMethod = authContext.HttpContext.Request.Method;

            switch (httpMethod)
            {
                case GET:
                    actionPart = READ;
                    break;
                case POST:
                    actionPart = CREATE;
                    break;
                case PUT:
                case PATCH:
                    actionPart = UPDATE;
                    break;
                case HttpMethods.DELETE:
                    actionPart = Operations.DELETE;
                    break;
                default:
                    break;
            }

            var resourcePart = actionDescriptor.ControllerName.ToLower();

            return $"{actionPart}-{resourcePart}";
        }
    }
}
