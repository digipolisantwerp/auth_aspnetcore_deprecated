using Microsoft.AspNetCore.Builder;

namespace Digipolis.Auth
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds Authentication and Authorization to the Microsoft.AspNetCore.Builder.IApplicationBuilder request execution pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            return app.UseAuthentication();
        }
    }
}
