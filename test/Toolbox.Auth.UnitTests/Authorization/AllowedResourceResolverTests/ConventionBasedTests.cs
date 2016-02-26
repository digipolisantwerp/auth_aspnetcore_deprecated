using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using Toolbox.Auth.Authorization;
using Xunit;

namespace Toolbox.Auth.UnitTests.Authorization.AllowedResourceResolverTests
{
    public class ConventionBasedTests
    {
        private readonly string _userId = "user123";

        [Fact]
        public void GetRequest()
        {
            var resolver = new AllowedResourceResolver();
            var context = CreateAuthorizationContext(typeof(ConventionsBasedResourceController), "Get", HttpMethod.Get);

            var allowedResources = resolver.ResolveFromConvention(context);

            Assert.Contains("read-conventionsbasedresource", allowedResources);
        }

        [Fact]
        public void PostRequest()
        {
            var resolver = new AllowedResourceResolver();
            var context = CreateAuthorizationContext(typeof(ConventionsBasedResourceController), "Post", HttpMethod.Post);

            var allowedResources = resolver.ResolveFromConvention(context);

            Assert.Contains("create-conventionsbasedresource", allowedResources);
        }

        [Fact]
        public void PutRequest()
        {
            var resolver = new AllowedResourceResolver();
            var context = CreateAuthorizationContext(typeof(ConventionsBasedResourceController), "Put", HttpMethod.Put);

            var allowedResources = resolver.ResolveFromConvention(context);

            Assert.Contains("update-conventionsbasedresource", allowedResources);
        }

        //[Fact]
        //public void PatchRequest()
        //{
        //    var resolver = new AllowedResourceResolver();
        //    var context = CreateAuthorizationContext(typeof(ResourceController), "Patch", HttpMethod.Pat);

        //    var allowedResources = resolver.ResolveFromConvention(context);

        //    Assert.Contains("write-conventionsbasedresource", allowedResources);
        //}

        [Fact]
        public void DeleteRequest()
        {
            var resolver = new AllowedResourceResolver();
            var context = CreateAuthorizationContext(typeof(ConventionsBasedResourceController), "Delete", HttpMethod.Delete);

            var allowedResources = resolver.ResolveFromConvention(context);

            Assert.Contains("delete-conventionsbasedresource", allowedResources);
        }

        private AuthorizationContext CreateAuthorizationContext(Type controllerType, string action, HttpMethod httpMethod)
        {
            var actionContext = new Microsoft.AspNet.Mvc.ActionContext();

            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest.Setup(r => r.Method)
                .Returns(httpMethod.Method);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request)
                .Returns(mockHttpRequest.Object);

            actionContext.HttpContext = mockHttpContext.Object;
            actionContext.RouteData = new Microsoft.AspNet.Routing.RouteData();

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                ControllerName = controllerType.Name.Remove(controllerType.Name.IndexOf("Controller"), 10),
                MethodInfo = controllerType.GetMethod(action)
            };
            actionContext.ActionDescriptor = actionDescriptor;

            var resource = new Microsoft.AspNet.Mvc.Filters.AuthorizationContext(actionContext, new List<Microsoft.AspNet.Mvc.Filters.IFilterMetadata>());

            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, _userId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationContext(requirements, user, resource);
            return context;
        }


    }
}
