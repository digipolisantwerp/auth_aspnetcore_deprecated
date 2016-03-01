namespace Toolbox.Auth.Options
{
    public class AuthOptions
    {
        /// <summary>
        /// The name of the application in which context the user is requesting a resource.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The url to the PDP endpoint.
        /// </summary>
        public string PdpUrl { get; set; }

        /// <summary>
        /// The duration in minutes the responses from the PDP are cached.
        /// Set to zero to disable caching.
        /// </summary>
        public int PdpCacheDuration { get; set; }

        /// <summary>
        /// The audience url used to validate the Jwt token.
        /// </summary>
        public string JwtAudience { get; set; }
        
        /// <summary>
        /// The issuer string used to validate the Jwt token.
        /// </summary>
        public string JwtIssuer { get; set; }

        /// <summary>
        /// The claim type used to store the user id in the Jwt token.
        /// Default = "sub".
        /// </summary>
        public string JwtUserIdClaimType { get; set; } = AuthOptionsDefaults.JwtUserIdClaimType;

    }
}
