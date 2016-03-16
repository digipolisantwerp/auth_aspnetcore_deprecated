using Microsoft.AspNet.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var firstPolicy = new AuthorizationPolicyBuilder()
                                .RequireClaim(Constants.CustomClaim)
                                .Build();

            policies.Add(Constants.UserWithCustomClaimOnly, firstPolicy);

            return policies;
        }
    }
}
