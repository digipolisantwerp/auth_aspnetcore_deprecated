using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Digipolis.Auth.Mvc;
using Digipolis.Auth.Options;
using Xunit;

namespace Digipolis.Auth.UnitTests.Mvc
{
    public class AuthActionsOptionsSetupTests
    {
        [Fact]
        public void AuthActionsConventionAdded()
        {
            var mvcOptions = new MvcOptions();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<AuthOptions>)))
                .Returns(Options.Create(new AuthOptions()));
            var convention = new AuthActionsOptionsSetup(mockServiceProvider.Object);

            convention.Configure(mvcOptions);

            Assert.True(mvcOptions.Conventions.Count == 1);
        }
    }
}
