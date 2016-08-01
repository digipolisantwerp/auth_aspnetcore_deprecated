using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Linq;
using Digipolis.Auth.Controllers;
using Digipolis.Auth.Options;

namespace Digipolis.Auth.Mvc
{
    public class AuthActionsConvention : IActionModelConvention
    {
        private readonly AuthOptions _authOptions;

        public AuthActionsConvention(AuthOptions options)
        {
            _authOptions = options;
        }

        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerType.FullName == typeof(TokenController).FullName || action.Controller.ControllerType.FullName == typeof(PermissionsController).FullName)
            {
                var selectorModel = action.Selectors.First();
                selectorModel.ActionConstraints.Add(new AuthActionsConstraint(_authOptions));

                var template = string.Empty;

                if (action.ActionName == "Callback")
                    template = _authOptions.TokenCallbackRoute;

                if (action.ActionName == "Refresh")
                    template = _authOptions.TokenRefreshRoute;

                if (action.ActionName == "GetPermissions")
                    template = _authOptions.PermissionsRoute;

                if (!String.IsNullOrWhiteSpace(template))
                {
                    selectorModel.AttributeRouteModel = new AttributeRouteModel()
                    {
                        Name = template,
                        Order = Int32.MaxValue,
                        Template = template
                    };
                }
            }
        }
        
        
            
    }
}
