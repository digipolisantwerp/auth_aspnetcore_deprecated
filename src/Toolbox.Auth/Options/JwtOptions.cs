using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolbox.Auth.Options
{
    public class JwtOptions
    {
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

        /// <summary>
        /// The Api Engine authentication token refresh url.
        /// </summary>
        public string ApiAuthTokenRefreshUrl { get; set; }

        /// <summary>
        /// Set to true to enable automatic token refresh.
        /// Only used with the cookie authentication scheme.
        /// </summary>
        public bool AutomaticTokenRefresh { get; set; } = false;

        /// <summary>
        /// The amount of minutes left on a jwt token expiration time in order to refresh the token.
        /// </summary>
        public int TokenRefreshTime { get; set; }

        /// <summary>
        /// The route used for the token callback url. Default = "token".
        /// </summary>
        public string TokenCallbackRoute { get; set; } = AuthOptionsDefaults.TokenCallbackRoute;
    }
}
