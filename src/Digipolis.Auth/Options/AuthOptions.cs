namespace Digipolis.Auth.Options
{
    public class AuthOptions
    {
        /// <summary>
        /// The name of the application in which context the user is requesting a resource.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The base url for the application, including scheme and eventual port.
        /// ex. https://test.antwerpen.be:443
        /// </summary>
        public string ApplicationBaseUrl { get; set; }

        /// <summary>
        /// Set to true to enable the cookie authentication handling. Default = false.
        /// </summary>
        public bool EnableCookieAuth { get; set; } = false;

        /// <summary>
        /// CookieAuth authentication ticket life time. Default = 480 (8 hours).
        /// </summary>
        public int CookieAuthLifeTime { get; set; } = 480;

        /// <summary>
        /// Set to true to enable the Jwt in header authentication handling. Default = true.
        /// </summary>
        public bool EnableJwtHeaderAuth { get; set; } = true;

        /// <summary>
        /// The url to the PDP endpoint.
        /// </summary>
        public string PdpUrl { get; set; }

        /// <summary>
        /// The api key for the PDP endpoint.
        /// </summary>
        public string PdpApiKey { get; set; }

        /// <summary>
        /// The duration in minutes the responses from the PDP are cached.
        /// Set to zero to disable caching. Default = 60.
        /// </summary>
        public int PdpCacheDuration { get; set; } = 60;

        /// <summary>
        /// Set to true to use a shared (external) dataprotection key store to store the key used by cookie auth.
        /// </summary>
        public bool UseDotnetKeystore { get; set; }
        
        /// <summary>
        /// Connection string for the shared dataprotection key store.
        /// </summary>
        public string DotnetKeystore { get; set; }

        /// <summary>
        /// The audience url used to validate the Jwt token.
        /// </summary>
        public string JwtAudience { get; set; } = string.Empty;

        /// <summary>
        /// The issuer string used to validate the Jwt token.
        /// </summary>
        public string JwtIssuer { get; set; }

        /// <summary>
        /// The duration in minutes the Jwt signing key is cached.
        /// Default = 1440 minutes (24 hours).
        /// </summary>
        public int JwtSigningKeyCacheDuration { get; set; } = 1440;

        /// <summary>
        /// Set to true to add the jwt token in a cookie.
        /// Default = true.
        /// </summary>
        public bool AddJwtCookie { get; set; } = true;

        /// <summary>
        /// Set to true to add the jwt token to the Http Session.
        /// This requires Sessions to be enabled and configured.
        /// Default = false;
        /// </summary>
        public bool AddJwtToSession { get; set; }

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

        /// <summary>
        /// The Api Engine authentication token refresh url.
        /// </summary>
        public string ApiAuthTokenRefreshUrl { get; set; }

        /// <summary>
        /// The Api Engine authentication logout url.
        /// </summary>
        public string ApiAuthTokenLogoutUrl { get; set; }

        /// <summary>
        /// Set to true to enable automatic token refresh.
        /// Only used with the cookie authentication scheme.
        /// </summary>
        public bool AutomaticTokenRefresh { get; set; } = false;

        /// <summary>
        /// The amount of minutes before the jwt token expiration time at which to automatically refresh the token.
        /// </summary>
        public int TokenRefreshTime { get; set; } = 5;

        /// <summary>
        /// The route used for the token callback url. Default = "auth/token".
        /// </summary>
        public string TokenCallbackRoute { get; set; } = AuthOptionsDefaults.TokenCallbackRoute;

        /// <summary>
        /// The route used for the token refresh endpoint. Default = "auth/token/refresh".
        /// </summary>
        public string TokenRefreshRoute { get; set; } = AuthOptionsDefaults.TokenRefreshRoute;

        /// <summary>
        /// The route used for the permissions endpoint. Default = "auth/user/permissions".
        /// </summary>
        public string PermissionsRoute { get; set; } = AuthOptionsDefaults.PermissionsRoute;

        /// <summary>
        /// The path to redirect when the access is denied.
        /// </summary>
        public string AccessDeniedPath { get; set; }
    }
}
