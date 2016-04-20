using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleApp.Policies;
using Toolbox.Auth;

namespace SampleApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            //Recomended way to store your secrets / passwords
            //builder.AddUserSecrets();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add authorization services with options and policies
            #region Code Based configuration

            //services.AddAuth(options =>
            //{
            //    options.ApplicationName = "SampleApp";
            //    options.PdpUrl = "http://localhost:5000/pdp";
            //    //options.ApplicationName = "JHUAT03";
            //    //options.PdpUrl = "https://esb-app1-o.antwerpen.be/authz/v1";
            //    options.PdpCacheDuration = 0; //No caching for the samples
            //    options.JwtAudience = "SampleApp";
            //    options.JwtIssuer = "5f75f0c6cf4d4c4f97dd0ab68ce534f4";
            //    //options.JwtIssuer = "Online JWT Builder";
            //    options.JwtSigningKeyProviderUrl = "http://localhost:5000/signingKey";
            //    options.JwtSigningKeyCacheDuration = 0;
            //    options.JwtSigningKeyProviderApikey = "yoursupersecretkey";         //don't do this in your code! just for demo purpose
            //                                                                        //options.jwtSigningKeyProviderApikey = Configuration["apikey"];    //using the recomended way (see https://docs.asp.net/en/latest/security/app-secrets.html)

            //    options.ApiAuthUrl = "http://devasu016.dev.digant.antwerpen.local/API-Engine-auth/v1/login/idp/redirect/proxied";
            //    options.ApiAuthIdpUrl = "https://identityserver-o.antwerpen.be/samlsso";
            //    options.ApiAuthSpName = "apiengine";
            //    options.ApiAuthSpUrl = "https://api-engine-o.antwerpen.be/API-Engine-auth/v1/login/idp/callback";

            //}, PolicyBuilder.Build());

            #endregion

            #region File based configuration

            services.AddAuth(configFile =>
            {
                configFile.FileName = "authconfig.json";
                configFile.Section = "Auth";
            }, PolicyBuilder.Build());

            #endregion

            services.AddMvc();

            //If you want to require an authenticated user for all endpoints you can use the setup as below
            //services.AddMvc(config =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //                    .RequireAuthenticatedUser()
            //                    .Build();
            //    config.Filters.Add(new AuthorizeFilter(policy));
            //});

            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            //Add authorization middleware
            app.UseAuth();

            app.UseSwaggerGen();
            app.UseSwaggerUi();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
