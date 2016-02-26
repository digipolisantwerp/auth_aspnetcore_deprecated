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
    public class AllowedResourceResolver : IAllowedResourceResolver
    {
        //public IEnumerable<string> ResolveAllowedResources(AuthorizationContext context)
        //{
        //    var authContext = context.Resource as Microsoft.AspNet.Mvc.Filters.AuthorizationContext;
        //    var actionDescriptor = authContext.ActionDescriptor as ControllerActionDescriptor;

        //    var allowedResources = ResolveFromAttributeProperties(actionDescriptor);

        //    if (!allowedResources.Any())
        //    {
        //        var resolvedResource = ResolveFromConvention(authContext, actionDescriptor);
        //        allowedResources.Add(resolvedResource);
        //    }

        //    return allowedResources;
        //}

        public IEnumerable<string> ResolveFromAttributeProperties(AuthorizationContext context)
        {
            var authContext = context.Resource as Microsoft.AspNet.Mvc.Filters.AuthorizationContext;
            var actionDescriptor = authContext.ActionDescriptor as ControllerActionDescriptor;

            var allowedResources = new List<string>();
            var authorizePermissionsAttribute = actionDescriptor.MethodInfo.CustomAttributes
                                                                .FirstOrDefault(a => a.AttributeType == typeof(AuthorizeWithAttribute));

            if (authorizePermissionsAttribute != null)
            {
                var customPermissions = authorizePermissionsAttribute.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.CustomPermissions))
                    .TypedValue.Value as ReadOnlyCollection<CustomAttributeTypedArgument>;

                if (customPermissions != null)
                    customPermissions.ToList<CustomAttributeTypedArgument>().ForEach(p => allowedResources.Add(p.Value.ToString()));

                var customPermission = authorizePermissionsAttribute.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.CustomPermission))
                    .TypedValue.Value?.ToString();

                if (customPermission != null)
                    allowedResources.Add(customPermission);
            }

            return allowedResources;
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
