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
    public class TokenControllerConventionTests
    {
        [Fact]
        private void OptionsIsSet()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "myroute" };

            var convention = new TokenControllerConvention(options);

            Assert.Same(options, convention.Options);
        }

        [Fact]
        private void RoutesAreSetForTokenControllerModel()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "token/callback", TokenRefreshRoute = "token/refresh" };

            var convention = new TokenControllerConvention(options);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());
            model.Actions.Add(new ActionModel(typeof(TokenController).GetMethod("Callback"), new List<object>()) { ActionName = "Callback" });
            model.Actions.Add(new ActionModel(typeof(TokenController).GetMethod("Refresh"), new List<object>()) { ActionName = "Refresh" });

            convention.Apply(model);

            Assert.Equal("token/callback", model.Actions.Single(a => a.ActionName == "Callback").AttributeRouteModel.Template);
            Assert.Equal("TokenCallbackRoute", model.Actions.Single(a => a.ActionName == "Callback").AttributeRouteModel.Name);

            Assert.Equal("token/refresh", model.Actions.Single(a => a.ActionName == "Refresh").AttributeRouteModel.Template);
            Assert.Equal("TokenRefreshRoute", model.Actions.Single(a => a.ActionName == "Refresh").AttributeRouteModel.Name);
        }

        [Fact]
        private void RouteIsNotSetForNonTokenControllerModel()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "myroute" };

            var convention = new TokenControllerConvention(options);
            var model = new ControllerModel(typeof(TestController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteNullIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = null };

            var convention = new TokenControllerConvention(options);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteEmptyIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "" };

            var convention = new TokenControllerConvention(options);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteWhitespaceIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "   " };

            var convention = new TokenControllerConvention(options);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }
    }
}
