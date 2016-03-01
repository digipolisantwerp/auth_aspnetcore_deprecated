using Microsoft.AspNet.Authorization;
using System;

namespace Toolbox.Auth.Authorization
{
    internal class ConventionBasedAuthorizationHandler : AuthorizationHandler<ConventionBasedRequirement>
    {
        private readonly IAllowedResourceResolver _resourceResolver;

        public ConventionBasedAuthorizationHandler(IAllowedResourceResolver resourceResolver)
        {
            if (resourceResolver == null) throw new ArgumentNullException(nameof(resourceResolver), $"{nameof(resourceResolver)} cannot be null");

            _resourceResolver = resourceResolver;
        }

        protected override void Handle(AuthorizationContext context, ConventionBasedRequirement requirement)
        {
            var allowedResource = _resourceResolver.ResolveFromConvention(context);

            if (context.User.HasClaim(Claims.PermissionsType, allowedResource))
                context.Succeed(requirement);
        }
    }
}
