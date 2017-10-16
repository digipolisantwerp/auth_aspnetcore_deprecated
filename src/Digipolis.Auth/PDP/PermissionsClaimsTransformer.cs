using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Digipolis.Auth.Options;
using System.Net;
using System.Reflection;

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

        private string CreateUserId(String applicationUserName) {
            var userWithOldDomain = applicationUserName.Split(new char[] { '\\' });
            var newDomain = GetNewDomain(userWithOldDomain[0]);
            var userWithNewDomain = userWithOldDomain[1] + newDomain;
            return userWithNewDomain;
        }

        private string GetNewDomain(string oldDomain)
        {
            var domain = String.Empty;
            var toMatch = oldDomain.Trim().ToUpper();

            Type type = typeof(ApplicationUserDomain);
            var flags = BindingFlags.Static | BindingFlags.Public;
            //checks to see if there exists a constantfield with the name of the old domain 
            var field = type.GetFields(flags).FirstOrDefault(f => toMatch.Equals(f.Name));
            if (field != null)
                //and returns its value (the new domain)
                domain = (string) field.GetValue(null);

            return domain;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsTransformationContext context)
        {
            var userId = String.Empty;
            if (context.Principal?.Identity?.Name == null || context.Principal?.Identities?.FirstOrDefault()?.HasClaim(c => c.Type == Claims.PermissionsType) == true) {
                if (context.Principal?.Identities?.FirstOrDefault()?.HasClaim(c => c.Type == Claims.XCredentialUserName) == true) {
                    var applicationUserName = context.Principal.Identities.FirstOrDefault().Claims.FirstOrDefault(claim => claim.Type == Claims.XCredentialUserName).Value;
                    userId = CreateUserId(applicationUserName);
                }
                else {
                    return context.Principal;
                }
            }
            else {
                userId = context.Principal.Identity.Name;
            }
                
  
            var pdpResponse = await _pdpProvider.GetPermissionsAsync(userId, _authOptions.ApplicationName);

            pdpResponse?.permissions?.ToList().ForEach(permission =>
            {
                context.Principal.Identities.First().AddClaim(new Claim(Claims.PermissionsType, permission));
            });

            return context.Principal;
        }
    }
}
