using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
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
            var authOptions = OptionsServices.GetService<IOptions<AuthOptions>>();
            options.Conventions.Add(new TokenControllerConvention(authOptions));
        }
    }
}
