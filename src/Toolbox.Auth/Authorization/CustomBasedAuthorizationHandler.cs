using Microsoft.AspNet.Authorization;
using System;
using System.Linq;

namespace Toolbox.Auth.Authorization
{
    internal class CustomBasedAuthorizationHandler : AuthorizationHandler<CustomBasedRequirement>
    {
        private readonly IRequiredPermissionsResolver _resourceResolver;

        public CustomBasedAuthorizationHandler(IRequiredPermissionsResolver resourceResolver)
        {
            if (resourceResolver == null) throw new ArgumentNullException(nameof(resourceResolver), $"{nameof(resourceResolver)} cannot be null");

            _resourceResolver = resourceResolver;
        }

        protected override void Handle(AuthorizationContext context, CustomBasedRequirement requirement)
        {
            var requiredPermissions = _resourceResolver.ResolveFromAttributeProperties(context);

            if (requiredPermissions.Any(r => context.User.HasClaim(Claims.PermissionsType, r)))
                context.Succeed(requirement);
        }
    }
}
