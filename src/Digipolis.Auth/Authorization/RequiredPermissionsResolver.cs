using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using static Digipolis.Auth.HttpMethods;
using static Digipolis.Auth.Operations;

namespace Digipolis.Auth.Authorization
{
    public class RequiredPermissionsResolver : IRequiredPermissionsResolver
    {
        public IEnumerable<string> ResolveFromAttributeProperties(AuthorizationHandlerContext context)
        {
            var endpoint = context.Resource as Endpoint;
            var requiredPermissions = new List<string>();
            var authorizationAttributes = endpoint.Metadata?.ToList() ?? new List<object>();

            for (var i = 0; i < authorizationAttributes.Count; i++)
            {
                var attribute = authorizationAttributes[i];
                var attributeData = (CustomAttributeData)attribute;
                if (attributeData.AttributeType != typeof(AuthorizeWithAttribute))
                {
                    continue;
                }

                var permissions = attributeData.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.Permissions))
                    .TypedValue.Value as ReadOnlyCollection<CustomAttributeTypedArgument>;

                if (permissions != null)
                    permissions.ToList<CustomAttributeTypedArgument>().ForEach(p => requiredPermissions.Add(p.Value.ToString()));

                var permission = attributeData.NamedArguments
                    .FirstOrDefault(a => a.MemberName == nameof(AuthorizeWithAttribute.Permission))
                    .TypedValue.Value?.ToString();

                if (permission != null)
                    requiredPermissions.Add(permission);
            }

            return requiredPermissions;
        }

        public string ResolveFromConvention(AuthorizationHandlerContext context)
        {
            var authContext = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;
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
