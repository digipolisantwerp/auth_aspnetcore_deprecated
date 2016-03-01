namespace Toolbox.Auth
{
    public static class Policies
    {
        public const string ConventionBased = "ConventionBased";
        public const string CustomBased = "CustomBased";
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

    internal static class Claims
    {
        public const string PermissionsType = "permissions";
    };
}
