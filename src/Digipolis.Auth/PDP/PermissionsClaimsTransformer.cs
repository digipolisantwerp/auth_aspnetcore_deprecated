using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Digipolis.Auth.Options;

namespace Digipolis.Auth.PDP
{
    public class PermissionsClaimsTransformer : IClaimsTransformer
    {
        private readonly AuthOptions _authOptions;
        private readonly IPolicyDescisionProvider _pdpProvider;

        public PermissionsClaimsTransformer(IOptions<AuthOptions> authOptions, IPolicyDescisionProvider pdpProvider)
        {
            if (authOptions == null || authOptions.Value == null) throw new ArgumentNullException(nameof(authOptions), $"{nameof(authOptions)} cannot be null");
            if (pdpProvider == null) throw new ArgumentNullException(nameof(pdpProvider), $"{nameof(pdpProvider)} cannot be null");

            _authOptions = authOptions.Value;
            _pdpProvider = pdpProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsTransformationContext context)
        {
            if (context.Principal?.Identity?.Name == null || context.Principal?.Identities?.FirstOrDefault()?.HasClaim(c => c.Type == Claims.PermissionsType) == true)
                return context.Principal;

            var userId = context.Principal.Identities.FirstOrDefault()?.Claims.SingleOrDefault(c => c.Type == Claims.Name)?.Value;

            var pdpResponse = await _pdpProvider.GetPermissionsAsync(userId, _authOptions.ApplicationName);

            pdpResponse?.permissions?.ToList().ForEach(permission =>
            {
                context.Principal.Identities.First().AddClaim(new Claim(Claims.PermissionsType, permission));
            });

            return context.Principal;
        }
    }
}
