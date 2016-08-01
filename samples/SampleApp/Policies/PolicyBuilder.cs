using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.Auth;
using Digipolis.Auth.PDP;

namespace SampleApp.Policies
{
    /// <summary>
    /// Class to build standard Microsoft.AspNet Authorization policies.
    /// These policies are not to be confused with the policies defined in the Identity Server and used by the PDP!
    /// </summary>
    public class PolicyBuilder
    {
        public static Dictionary<string, AuthorizationPolicy> Build()
        {
            var policies = new Dictionary<string, AuthorizationPolicy>();

            var appUserPolicy = new AuthorizationPolicyBuilder()
                                .RequireClaim(Claims.PermissionsType, Constants.ApplicationLoginPermission)
                                .Build();

            policies.Add(Constants.ApplicationUser, appUserPolicy);

            var customPolicy = new AuthorizationPolicyBuilder()
                                .AddAuthenticationSchemes(AuthSchemes.JwtHeaderAuth)
                                .RequireClaim(Constants.CustomClaim)
                                .Build();

            policies.Add(Constants.UserWithCustomClaimOnly, customPolicy);

            return policies;
        }
    }
}
