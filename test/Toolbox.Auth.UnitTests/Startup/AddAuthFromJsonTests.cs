using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Linq;
using System.Net.Http;
using Toolbox.Auth.Authorization;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;
using Xunit;

namespace Toolbox.Auth.UnitTests.Startup
{
    public class AddAuthFromJsonTests : AddAuthBaseTests
    {
        public AddAuthFromJsonTests()
        {
            Act = services =>
            {
                services.AddAuth(options =>
                {
                    options.FileName = @"_TestData/authconfig.json";
                    options.Section = "Auth";
                });
            };
        }
    }
}
