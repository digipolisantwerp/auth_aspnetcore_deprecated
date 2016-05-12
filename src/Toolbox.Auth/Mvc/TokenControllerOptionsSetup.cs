using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using System;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
{
    public class TokenControllerOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public TokenControllerOptionsSetup(IServiceProvider serviceProvider)
        {
            OptionsServices = serviceProvider;
        }

        internal IServiceProvider OptionsServices { get; private set; }

        public void Configure(MvcOptions options)
        {
            var authOptions = OptionsServices.GetService<IOptions<AuthOptions>>().Value;
            options.Conventions.Add(new TokenControllerConvention(authOptions));
            options.Conventions.Add(new AuthActionsConvention(authOptions));
        }
    }
}
