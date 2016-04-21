using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.Extensions.OptionsModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.Auth.Jwt;
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
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);

            Assert.Same(options, convention.Options);
        }

        [Fact]
        private void RouteIsSetForTokenControllerModel()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "myroute" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(1, model.AttributeRoutes.Count);
            Assert.Equal("myroute", model.AttributeRoutes.First().Template);
            Assert.Equal("TokenCallbackRoute", model.AttributeRoutes.First().Name);
        }

        [Fact]
        private void RouteIsNotSetForNonTokenControllerModel()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "myroute" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TestController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteNullIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = null };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteEmptyIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "" };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }

        [Fact]
        private void RouteWhitespaceIsNotSetForTokenController()
        {
            var options = new AuthOptions() { TokenCallbackRoute = "   " };
            var optionsMock = new Mock<IOptions<AuthOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var convention = new TokenControllerConvention(optionsMock.Object);
            var model = new ControllerModel(typeof(TokenController).GetTypeInfo(), new List<object>());

            convention.Apply(model);

            Assert.Equal(0, model.AttributeRoutes.Count);
        }
    }
}
