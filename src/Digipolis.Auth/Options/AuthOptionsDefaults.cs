namespace Digipolis.Auth.Options
{
    public static class AuthOptionsDefaults
    {
        public const string OptionsFileName = "authconfig.json";
        public const string OptionsFileAuthSection = "Auth";
        public const string TokenCallbackRoute = "auth/token";
        public const string TokenRefreshRoute = "auth/token/refresh";
        public const string PermissionsRoute = "auth/user/permissions";
        public const string FrontEndApiRouteIdentifier = "api";
        public const string JwtTokenSource = "session";
    }
}
