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
    public class PermissionsControllerConventionTests
    {
        [Fact]
        private void OptionsIsSet()
        {
            var options = new AuthOptions() { PermissionsRoute = "myroute" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);

            Assert.Same(options, convention.Options);
        }

        [Fact]
        private void RouteIsSetForPermissionsControllerModel()
        {
            var options = new AuthOptions() { PermissionsRoute = "myroute" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(PermissionsController).GetTypeInfo(), new List<object>());
            model.Actions.Add(new ActionModel(typeof(PermissionsController).GetMethod("GetPermissions"), new List<object>()) { ActionName = "GetPermissions" });

            convention.Apply(model);

            Assert.Equal("myroute", model.Actions.Single(a => a.ActionName == "GetPermissions").AttributeRouteModel.Template);
            Assert.Equal("PermissionsRoute", model.Actions.Single(a => a.ActionName == "GetPermissions").AttributeRouteModel.Name);
        }

        [Fact]
        private void RouteIsNotSetForNonPermissionsControllerModel()
        {
            var options = new AuthOptions() { PermissionsRoute = "myroute" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TestController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteNullIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = null };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(PermissionsController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteEmptyIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = "" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(PermissionsController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteWhitespaceIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = "   " };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new PermissionsControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(PermissionsController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }
    }
}
