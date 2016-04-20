namespace Toolbox.Auth.Options
{
    public class AuthOptions
    {
        /// <summary>
        /// The name of the application in which context the user is requesting a resource.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Set to true to enable the cookie authentication handling. Default = false.
        /// </summary>
        public bool EnableCookieAuth { get; set; } = false;

        /// <summary>
        /// Set to true to enable the Jwt in header authentication handling. Default = true.
        /// </summary>
        public bool EnableJwtHeaderAuth { get; set; } = true;

        /// <summary>
        /// The url to the PDP endpoint.
        /// </summary>
        public string PdpUrl { get; set; }

        /// <summary>
        /// The duration in minutes the responses from the PDP are cached.
        /// Set to zero to disable caching. Default = 60.
        /// </summary>
        public int PdpCacheDuration { get; set; } = 60;

        /// <summary>
        /// The audience url used to validate the Jwt token.
        /// </summary>
        public string JwtAudience { get; set; }
        
        /// <summary>
        /// The issuer string used to validate the Jwt token.
        /// </summary>
        public string JwtIssuer { get; set; }

        /// <summary>
        /// The url to the Jwt signing key endpoint.
        /// </summary>
        public string JwtSigningKeyProviderUrl { get; set; }

        /// <summary>
        /// The api key for the signing key provider authentication.
        /// </summary>
        public string JwtSigningKeyProviderApikey { get; set; }
        
        /// <summary>
        /// The duration in minutes the Jwt signing key is cached.
        /// Default = 10 minutes.
        /// </summary>
        public int JwtSigningKeyCacheDuration { get; set; } = 10;

        /// <summary>
        /// The clock skew in minutes to apply for the Jwt expiration validation.
        /// Default = 0 minutes.
        /// </summary>
        public int JwtValidatorClockSkew { get; set; } = 0;

        /// <summary>
        /// The url for the Api Engine authentication endpoint.
        /// </summary>
        public string ApiAuthUrl { get; set; }

        /// <summary>
        /// The url of the Idp the Api Engine will redirect the saml request to.
        /// </summary>
        public string ApiAuthIdpUrl { get; set; }

        /// <summary>
        /// The  service provider name of the Api Engine.
        /// </summary>
        public string ApiAuthSpName { get; set; }

        /// <summary>
        /// The Api Engine callback url where the idp must redirect to.
        /// </summary>
        public string ApiAuthSpUrl { get; set; }
        
    }
}
