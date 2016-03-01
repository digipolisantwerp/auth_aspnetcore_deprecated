using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
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

            var jwtBearerOptions = CreateJwtOptions(authOptions);

            app.UseJwtBearerAuthentication(jwtBearerOptions);

            var claimsTransformer = app.ApplicationServices.GetService<PermissionsClaimsTransformer>();

            app.UseClaimsTransformation(options =>
            {
                options.Transformer = claimsTransformer;
            });

            return app;
        }

        private static JwtBearerOptions CreateJwtOptions(AuthOptions authOptions)
        {
            var jwtBearerOptions = new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
            };

            jwtBearerOptions.TokenValidationParameters.ValidateActor = false;

            jwtBearerOptions.TokenValidationParameters.ValidateAudience = false;
            jwtBearerOptions.TokenValidationParameters.ValidAudience = authOptions.JwtAudience;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuer = false;
            jwtBearerOptions.TokenValidationParameters.ValidIssuer = authOptions.JwtIssuer;

            jwtBearerOptions.TokenValidationParameters.ValidateLifetime = false;

            jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey = false;
            jwtBearerOptions.TokenValidationParameters.ValidateSignature = false;

            return jwtBearerOptions;
        }
    }
}
