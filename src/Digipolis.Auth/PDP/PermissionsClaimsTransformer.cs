using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public class PermissionsClaimsTransformer : IClaimsTransformation
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

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.Name == null || principal?.Identities?.FirstOrDefault()?.HasClaim(c => c.Type == Claims.PermissionsType) == true)
                return principal;

            var userId = principal.Identity.Name;

            var pdpResponse = await _pdpProvider.GetPermissionsAsync(userId, _authOptions.ApplicationName);

            pdpResponse?.permissions?.ToList().ForEach(permission =>
            {
                principal.Identities.First().AddClaim(new Claim(Claims.PermissionsType, permission));
            });

            return principal;
        }
    }
}
