using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Threading.Tasks;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth.Authorization
{
    internal class ConventionBasedAuthorizationHandler : AuthorizationHandler<ConventionBasedRequirement>
    {
        private readonly IPolicyDescisionProvider _pdpProvider;
        private readonly AuthOptions _authOptions;
        private readonly IAllowedResourceResolver _resourceResolver;

        public ConventionBasedAuthorizationHandler(IPolicyDescisionProvider pdpProvider, IOptions<AuthOptions> authOptions, IAllowedResourceResolver resourceResolver)
        {
            if (pdpProvider == null) throw new ArgumentNullException(nameof(pdpProvider), $"{nameof(pdpProvider)} cannot be null");
            if (authOptions == null || authOptions.Value == null) throw new ArgumentNullException(nameof(authOptions), $"{nameof(authOptions)} cannot be null");
            if (resourceResolver == null) throw new ArgumentNullException(nameof(resourceResolver), $"{nameof(resourceResolver)} cannot be null");

            _pdpProvider = pdpProvider;
            _authOptions = authOptions.Value;
            _resourceResolver = resourceResolver;
        }

        protected override void Handle(AuthorizationContext context, ConventionBasedRequirement requirement)
        {
            throw new NotImplementedException();
        }

        protected override async Task HandleAsync(AuthorizationContext context, ConventionBasedRequirement requirement)
        {
            var allowedResource = _resourceResolver.ResolveFromConvention(context);

            var hasAccess = await _pdpProvider.HasAccessAsync(context.User.Identity.Name, _authOptions.ApplicationName, allowedResource);

            if (hasAccess)
                context.Succeed(requirement);
        }
    }
}
