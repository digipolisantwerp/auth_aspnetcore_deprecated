using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
{
    public class PermissionsControllerOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public PermissionsControllerOptionsSetup(IServiceProvider serviceProvider)
        {
            OptionsServices = serviceProvider;
        }

        internal IServiceProvider OptionsServices { get; private set; }

        public void Configure(MvcOptions options)
        {
            var authOptions = OptionsServices.GetService<IOptions<AuthOptions>>();
            options.Conventions.Add(new PermissionsControllerConvention(authOptions));
        }
    }
}
