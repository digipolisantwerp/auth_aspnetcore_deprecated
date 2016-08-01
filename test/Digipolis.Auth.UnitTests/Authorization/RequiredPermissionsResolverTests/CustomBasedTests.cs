using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Digipolis.Auth.Authorization;
using Xunit;

namespace Digipolis.Auth.UnitTests.Authorization.ResolverTests
{
    public class CustomBasedTests
    {
        private readonly string _userId = "user123";

        [Fact]
        public void ResolveSingleCustomPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(CustomBasedResourceController), "GetSingle");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(2, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
            Assert.Contains("custompermission", requiredPermissions);
        }

        [Fact]
        public void ResolveMultipleCustomPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(CustomBasedResourceController), "GetMultiple");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(3, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
            Assert.Contains("permission1", requiredPermissions);
            Assert.Contains("permission2", requiredPermissions);
        }

        [Fact]
        public void ResolveControllerPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationHandlerContext(typeof(CustomBasedResourceController), "Get");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(1, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
        }

        private AuthorizationHandlerContext CreateAuthorizationHandlerContext(Type controllerType, string action)
        {
            var actionContext = new ActionContext();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request)
                .Returns(Mock.Of<HttpRequest>());

            actionContext.HttpContext = mockHttpContext.Object;
            actionContext.RouteData = new RouteData();

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
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
