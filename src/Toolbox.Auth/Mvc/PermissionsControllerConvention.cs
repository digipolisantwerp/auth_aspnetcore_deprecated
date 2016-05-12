using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Linq;
using Toolbox.Auth.Controllers;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
{
    public class PermissionsControllerConvention : IControllerModelConvention
    {
        public PermissionsControllerConvention(IOptions<AuthOptions> options)
        {
            Options = options.Value;
        }

        public AuthOptions Options { get; set; }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.FullName == typeof(PermissionsController).FullName && !String.IsNullOrWhiteSpace(Options.PermissionsRoute))
            {
                controller.Actions.Single(a => a.ActionName == "GetPermissions").AttributeRouteModel = new AttributeRouteModel()
                {
                    Name = "PermissionsRoute",
                    Order = 0,
                    Template = Options.PermissionsRoute
                };
            }
        }
    }
}
