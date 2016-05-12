using Microsoft.AspNet.Mvc.ApplicationModels;
using Toolbox.Auth.Controllers;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
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
                action.ActionConstraints.Add(new AuthActionsConstraint(_authOptions));
            }
        }
    }
}
