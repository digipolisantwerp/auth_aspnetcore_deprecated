using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace Digipolis.Auth.Authorization
{
    internal class ConventionBasedAuthorizationHandler : AuthorizationHandler<ConventionBasedRequirement>
    {
        private readonly IRequiredPermissionsResolver _resourceResolver;

        public ConventionBasedAuthorizationHandler(IRequiredPermissionsResolver resourceResolver)
        {
            if (resourceResolver == null) throw new ArgumentNullException(nameof(resourceResolver), $"{nameof(resourceResolver)} cannot be null");

            _resourceResolver = resourceResolver;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ConventionBasedRequirement requirement)
        {
            var requiredPermission = _resourceResolver.ResolveFromConvention(context);

            if (context.User.HasClaim(Claims.PermissionsType, requiredPermission))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
