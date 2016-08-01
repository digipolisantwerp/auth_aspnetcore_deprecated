using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using Digipolis.Auth.Authorization;
using Xunit;

namespace Digipolis.Auth.UnitTests.Authorization.ResolverTests
{
    public class ConventionBasedTests
    {
        private readonly string _userId = "user123";

        [Fact]
        public void ActionLevelGetRequest()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(ConventionsBasedResourceController), "Get", HttpMethod.Get);

            var requiredPermissions = resolver.ResolveFromConvention(context);

            Assert.Equal("read-conventionsbasedresource", requiredPermissions);
        }

        [Fact]
        public void ActionLevelPostRequest()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(ConventionsBasedResourceController), "Post", HttpMethod.Post);

            var requiredPermissions = resolver.ResolveFromConvention(context);

            Assert.Equal("create-conventionsbasedresource", requiredPermissions);
        }

        [Fact]
        public void ActionLevelPutRequest()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(ConventionsBasedResourceController), "Put", HttpMethod.Put);

            var requiredPermissions = resolver.ResolveFromConvention(context);

            Assert.Equal("update-conventionsbasedresource", requiredPermissions);
        }

        [Fact]
        public void ActionLevelDeleteRequest()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(ConventionsBasedResourceController), "Delete", HttpMethod.Delete);

            var requiredPermissions = resolver.ResolveFromConvention(context);

            Assert.Equal("delete-conventionsbasedresource", requiredPermissions);
        }

        private AuthorizationHandlerContext CreateAuthorizationHandlerContext(Type controllerType, string action, HttpMethod httpMethod)
        {
            var actionContext = new ActionContext();

            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest.Setup(r => r.Method)
                .Returns(httpMethod.Method);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request)
                .Returns(mockHttpRequest.Object);

            actionContext.HttpContext = mockHttpContext.Object;
            actionContext.RouteData = new RouteData();

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                ControllerName = controllerType.Name.Remove(controllerType.Name.IndexOf("Controller"), 10),
                MethodInfo = controllerType.GetMethod(action)
            };
            actionContext.ActionDescriptor = actionDescriptor;

            var resource = new Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, _userId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationHandlerContext(requirements, user, resource);
            return context;
        }


    }
}
