using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Digipolis.Auth.Utilities
{
    public class EnvironmentHelper
    {
        public static bool IsRequiredEnvironment(IServiceCollection services, string requiredEnvironment)
        {
            using (var sp = services.BuildServiceProvider())
            {
                var hostingEnvironment = sp.GetRequiredService<IHostingEnvironment>();
                return hostingEnvironment.IsEnvironment(requiredEnvironment);
            }
        }

        public static bool IsDevelopmentOrRequiredEnvironment(IServiceCollection services, string requiredEnvironment)
        {
            using (var sp = services.BuildServiceProvider())
            {
                var hostingEnvironment = sp.GetRequiredService<IHostingEnvironment>();
                return hostingEnvironment.IsDevelopment() || hostingEnvironment.IsEnvironment(requiredEnvironment);
            }
        }

        public static bool IsDevelopmentEnvironment(IServiceCollection services)
        {
            using (var sp = services.BuildServiceProvider())
            {
                var hostingEnvironment = sp.GetRequiredService<IHostingEnvironment>();
                return hostingEnvironment.IsDevelopment();
            }
        }
    }
}
