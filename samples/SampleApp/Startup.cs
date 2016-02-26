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
                options.PdpUrl = "http://localhost:5000";
                options.PdpCacheDuration = 0; //No caching for the samples
                options.JwtAudienceUrl = "";
                options.jwtValidIssuer = "SampleIDP";
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

            var options = new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
            };

            options.TokenValidationParameters.ValidateActor = false;

            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidAudience = "www.example.com";

            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.ValidIssuer = "Online JWT Builder";

            options.TokenValidationParameters.ValidateLifetime = false;

            options.TokenValidationParameters.ValidateIssuerSigningKey = false;
            options.TokenValidationParameters.ValidateSignature = false;

            app.UseJwtBearerAuthentication(options);

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
