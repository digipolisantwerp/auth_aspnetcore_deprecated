using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds Authentication and Authorization to the Microsoft.AspNet.Builder.IApplicationBuilder request execution pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            var authOptions = app.ApplicationServices.GetService<IOptions<AuthOptions>>().Value ;
            var signingKeyResolver = app.ApplicationServices.GetService<IJwtSigningKeyProvider>();
            var signatureValidator = app.ApplicationServices.GetService<IJwtTokenSignatureValidator>();

            var jwtBearerOptions = JwtBearerOptionsFactory.Create(authOptions, signingKeyResolver, signatureValidator);

            app.UseJwtBearerAuthentication(jwtBearerOptions);

            var claimsTransformer = app.ApplicationServices.GetService<PermissionsClaimsTransformer>();

            app.UseClaimsTransformation(options =>
            {
                options.Transformer = claimsTransformer;
            });

            return app;
        }
    }
}
