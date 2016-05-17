using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Linq;
using Toolbox.Auth.Controllers;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
{
    public class TokenControllerConvention : IControllerModelConvention
    {
        public TokenControllerConvention(AuthOptions options)
        {
            Options = options;
        }

        public AuthOptions Options { get; set; }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.FullName == typeof(TokenController).FullName && !String.IsNullOrWhiteSpace(Options.TokenCallbackRoute))
            {
                controller.Actions.Single(a => a.ActionName == "Callback").Selectors.Add(new SelectorModel()
                {
                    AttributeRouteModel = new AttributeRouteModel()
                    {
                        Name = "TokenCallbackRoute",
                        Order = 0,
                        Template = Options.TokenCallbackRoute
                    }
                });

                controller.Actions.Single(a => a.ActionName == "Refresh").Selectors.Add(new SelectorModel()
                {
                    AttributeRouteModel = new AttributeRouteModel()
                    {
                        Name = "TokenRefreshRoute",
                        Order = 0,
                        Template = Options.TokenRefreshRoute
                    }
                });
            }
        }
    }
}
