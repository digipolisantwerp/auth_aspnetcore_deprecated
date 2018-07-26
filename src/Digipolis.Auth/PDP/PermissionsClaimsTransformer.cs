using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public class PermissionsClaimsTransformer : IClaimsTransformation
    {
        private readonly IPermissionApplicationNameProvider _permissionApplicationNameProvider;
        private readonly IPolicyDescisionProvider _pdpProvider;

        public PermissionsClaimsTransformer(IPermissionApplicationNameProvider permissionApplicationNameProvider, IPolicyDescisionProvider pdpProvider)
        {
            if(permissionApplicationNameProvider == null) throw new ArgumentNullException(nameof(permissionApplicationNameProvider), $"{nameof(permissionApplicationNameProvider)} cannot be null");
            if (pdpProvider == null) throw new ArgumentNullException(nameof(pdpProvider), $"{nameof(pdpProvider)} cannot be null");

            _permissionApplicationNameProvider = permissionApplicationNameProvider;
            _pdpProvider = pdpProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.Name == null || principal?.Identities?.FirstOrDefault()?.HasClaim(c => c.Type == Claims.PermissionsType) == true)
                return principal;

            var userId = principal.Identity.Name;

            var pdpResponse = await _pdpProvider.GetPermissionsAsync(userId, _permissionApplicationNameProvider.ApplicationName());

            pdpResponse?.permissions?.ToList().ForEach(permission =>
            {
                principal.Identities.First().AddClaim(new Claim(Claims.PermissionsType, permission));
            });

            return principal;
        }
    }
}
