using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SamplePDP
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            //app.UseIISPlatformHandler();

            app.Run(async (context) =>
            {
                context.Response.Headers.Add("Content-Type", "application/json");
                                
                if (context.Request.Path.Value.Contains("pdp"))
                {
                    //User has convention based permissions
                    //await context.Response.WriteAsync("{'applicationid':'SampleAPP','userid':'user123','permissions':['read-tickets','create-tickets','update-tickets','delete-tickets']}");
                    
                    //User has a non convention based permission
                    await context.Response.WriteAsync("{'applicationid':'SampleAPP','userid':'user123','permissions':['read-tickets','create-tickets','update-tickets','delete-tickets', 'permission-125']}");

                    //User has no permissions
                    //await context.Response.WriteAsync("{'applicationid':'SampleAPP','userid':'user123','permissions':[]}");
                }

                if (context.Request.Path.Value.Contains("signingKey"))
                {
                    await context.Response.WriteAsync("secret");
                }


            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
