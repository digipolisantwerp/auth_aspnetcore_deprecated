using Microsoft.AspNet.Authorization;
using System;

namespace Toolbox.Auth.Authorization
{
    internal class ConventionBasedAuthorizationHandler : AuthorizationHandler<ConventionBasedRequirement>
    {
        private readonly IRequiredPermissionsResolver _resourceResolver;

        public ConventionBasedAuthorizationHandler(IRequiredPermissionsResolver resourceResolver)
        {
            if (resourceResolver == null) throw new ArgumentNullException(nameof(resourceResolver), $"{nameof(resourceResolver)} cannot be null");

            _resourceResolver = resourceResolver;
        }

        protected override void Handle(AuthorizationContext context, ConventionBasedRequirement requirement)
        {
            var requiredPermission = _resourceResolver.ResolveFromConvention(context);

            if (context.User.HasClaim(Claims.PermissionsType, requiredPermission))
                context.Succeed(requirement);
        }
    }
}
