using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.Extensions.OptionsModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.Auth.Controllers;
using Toolbox.Auth.Jwt;
using Toolbox.Auth.Mvc;
using Toolbox.Auth.Options;
using Toolbox.Auth.UnitTests.Utilities;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class AuthActionsConventionTests
    {
        [Fact]
        private void ConstraintIsAddedForTokenControllerModel()
        {
            var options = new AuthOptions() {  };

            var convention = new AuthActionsConvention(options);
            var controllerModel = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());
            var actionModel = new ActionModel (typeof(TokenController).GetMethods().First(), new List<object>());
            actionModel.Controller = controllerModel;

            convention.Apply(actionModel);

            Assert.True(actionModel.ActionConstraints.First() is AuthActionsConstraint);
        }

        [Fact]
        private void ConstraintIsAddedForPermissionsControllerModel()
        {
            var options = new AuthOptions() { };

            var convention = new AuthActionsConvention(options);
            var controllerModel = new ControllerModel(typeof(PermissionsController).GetTypeInfo(), new List<object>());
            var actionModel = new ActionModel(typeof(PermissionsController).GetMethods().First(), new List<object>());
            actionModel.Controller = controllerModel;

            convention.Apply(actionModel);

            Assert.True(actionModel.ActionConstraints.First() is AuthActionsConstraint);
        }

    }
}
