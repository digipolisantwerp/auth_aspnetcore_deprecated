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
    public class AddAuthFromOptionsTests : AddAuthBaseTests
    {
        public AddAuthFromOptionsTests()
        {
            Act = services =>
            {
                services.AddAuth(options =>
                {
                    options.ApplicationName = "AppName";
                    options.PdpUrl = "http://test.pdp.be/";
                    options.PdpCacheDuration = 60;
                    options.JwtAudience = "audience";
                    options.JwtIssuer = "issuer";
                    options.JwtUserIdClaimType = "sub";
                });
            };
        }
    }
}
