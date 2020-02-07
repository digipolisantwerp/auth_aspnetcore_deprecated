using Digipolis.ApplicationServices;
using Digipolis.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleApp.Policies;
using Microsoft.Extensions.Logging.Debug;

namespace SampleApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddApplicationServices(setup =>
            {
                setup.ApplicationName = "AUTHTOOLBOXSAMPLEAPP";
                setup.ApplicationId = "4830EF90-6252-4EDD-BD44-BC50D0B4F3D4";
            });

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
                configFile.BasePath = HostingEnvironment.ContentRootPath;
                configFile.FileName = "authconfig.json";
            }, PolicyBuilder.Build());

            #endregion

            services.AddSession();

            // Add framework services.
            services.AddMvc();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggingBuilder loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //Add authorization middleware
            app.UseAuth();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
