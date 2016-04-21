using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Jwt
{
    public class TokenControllerConvention : IControllerModelConvention
    {
        public TokenControllerConvention(IOptions<AuthOptions> options)
        {
            Options = options.Value;
        }

        public AuthOptions Options { get; set; }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.FullName == "Toolbox.Auth.Jwt.TokenController" && !String.IsNullOrWhiteSpace(Options.TokenCallbackRoute))
            {
                controller.AttributeRoutes.Clear();
                controller.AttributeRoutes.Add(new AttributeRouteModel() { Name = "TokenCallbackRoute", Order = 0, Template = Options.TokenCallbackRoute });
            }
        }
    }
}
