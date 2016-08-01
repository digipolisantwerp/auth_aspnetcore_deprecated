using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Digipolis.Auth.Options;

namespace Digipolis.Auth.Mvc
{
    public class AuthActionsOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public AuthActionsOptionsSetup(IServiceProvider serviceProvider)
        {
            OptionsServices = serviceProvider;
        }

        internal IServiceProvider OptionsServices { get; private set; }

        public void Configure(MvcOptions options)
        {
            var authOptions = OptionsServices.GetService<IOptions<AuthOptions>>().Value;
            options.Conventions.Add(new AuthActionsConvention(authOptions));
        }
    }
}
