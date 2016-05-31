using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Toolbox.Auth.Controllers;
using Toolbox.Auth.Mvc;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Jwt
{
    public class AuthActionsConventionTests
    {
        [Fact]
        private void ConstraintIsAddedForTokenControllerModel()
        {
            var options = new AuthOptions() {  };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.True(actionModel.Selectors.First().ActionConstraints.First() is AuthActionsConstraint);
        }

        [Fact]
        private void ConstraintIsAddedForPermissionsController()
        {
            var options = new AuthOptions() { };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(PermissionsController), "GetPermissions", options);

            Assert.True(actionModel.Selectors.First().ActionConstraints.First() is AuthActionsConstraint);
        }

        [Fact]
        private void RouteIsSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = "myroute" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(PermissionsController), "GetPermissions", options);

            Assert.Equal("myroute", actionModel.Selectors.First().AttributeRouteModel.Template);
            Assert.Equal("myroute", actionModel.Selectors.First().AttributeRouteModel.Name);
        }

        [Fact]
        private void RouteIsNotSetForNonPermissionsControllerModel()
        {
            var options = new AuthOptions() { PermissionsRoute = "myroute", TokenCallbackRoute = "" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RouteNullIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = null };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(PermissionsController), "GetPermissions", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RouteEmptyIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = "" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(PermissionsController), "GetPermissions", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RouteWhitespaceIsNotSetForPermissionsController()
        {
            var options = new AuthOptions() { PermissionsRoute = "   " };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(PermissionsController), "GetPermissions", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RoutesAreSetForTokenControllerCallbackAction()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "token/callback", TokenRefreshRoute = "token/refresh" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.Equal("token/callback", actionModel.Selectors.First().AttributeRouteModel.Template);
            Assert.Equal("token/callback", actionModel.Selectors.First().AttributeRouteModel.Name);
        }

        [Fact]
        private void RoutesAreSetForTokenControllerRefreshAction()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "token/callback", TokenRefreshRoute = "token/refresh" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Refresh", options);

            Assert.Equal("token/refresh", actionModel.Selectors.First().AttributeRouteModel.Template);
            Assert.Equal("token/refresh", actionModel.Selectors.First().AttributeRouteModel.Name);
        }

        [Fact]
        private void RouteNullIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = null };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RouteEmptyIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "" };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        [Fact]
        private void RouteWhitespaceIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "   " };

            var actionModel = CreateAndApplyAuthActionConvention(typeof(TokenController), "Callback", options);

            Assert.Null(actionModel.Selectors.First().AttributeRouteModel);
        }

        private ActionModel CreateAndApplyAuthActionConvention(Type controllerType, string actionMethodName, AuthOptions options)
        {
            var convention = new AuthActionsConvention(options);
            var controllerModel = new ControllerModel(controllerType.GetTypeInfo(), new List<object>());
            var actionModel = new ActionModel(controllerType.GetMethods().First(m => m.Name == actionMethodName), new List<object>());
            actionModel.ActionName = actionMethodName;
            actionModel.Controller = controllerModel;
            actionModel.Selectors.Add(new SelectorModel());

            convention.Apply(actionModel);

            return actionModel;
        }
    }
}
