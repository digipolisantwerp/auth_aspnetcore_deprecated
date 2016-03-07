using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Auth;
using Microsoft.AspNet.Authentication.JwtBearer;
using Toolbox.Auth.PDP;
using Microsoft.Extensions.OptionsModel;
using Toolbox.Auth.Options;
using System.Security.Claims;

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

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();

            //Add authorization services with options
            services.AddAuth(options =>
            {
                options.ApplicationName = "SampleApp";
                options.PdpUrl = "http://localhost:5000/pdp";
                options.PdpCacheDuration = 0; //No caching for the samples
                options.JwtAudience = "SampleApp";
                options.JwtIssuer = "Online JWT Builder";
                options.JwtSigningKeyProviderUrl = "http://localhost:5000/signingKey";
                options.JwtSigningKeyCacheDuration = 0;
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            //Add authorization middleware
            app.UseAuth();

            app.UseStaticFiles();

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
