namespace Digipolis.Auth
{
    public static class Policies
    {
        public const string ConventionBased = "ConventionBased";
        public const string CustomBased = "CustomBased";
        public const string SignalRBased = "SignalRBased";
    }

    public static class AuthSchemes
    {
        public const string CookieAuth = "CookieAuth";
        public const string JwtHeaderAuth = "JwtHeaderAuth";
    }

    internal static class HttpMethods
    {
        public const string GET = "GET";
        public const string POST = "POST";
        public const string PUT = "PUT";
        public const string PATCH = "PATCH";
        public const string DELETE = "DELETE";
    }

    internal static class Operations
    {
        public const string READ = "read";
        public const string CREATE = "create";
        public const string UPDATE = "update";
        public const string DELETE = "delete";
    }

    public static class Claims
    {
        public const string PermissionsType = "permissions";
        public const string Sub = "sub";
        public const string Name = "name";
        public const string XConsumerUsername = "X-Consumer-Username";
        public const string XAuthenticatedUserId = "X-Authenticated-Userid";
    };

    internal static class HeaderKeys
    {
        public const string Apikey = "apikey";
    }

    internal static class JWTTokenKeys
    {
        public const string Cookie = "jwt";
        public const string Session = "auth-jwt";
    }

    internal static class JwtTokenSource
    {
        public const string Cookie = "cookie";
        public const string Session = "session";
        public const string Header = "header";
    }
}
