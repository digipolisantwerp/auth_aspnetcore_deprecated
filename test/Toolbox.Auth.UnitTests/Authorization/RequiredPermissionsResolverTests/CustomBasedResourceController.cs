using Microsoft.AspNet.Mvc;
using Toolbox.Auth.Authorization;

namespace Toolbox.Auth.UnitTests.Authorization.ResolverTests
{
    [AuthorizeWith(Permission = "controllerpermission")]
    public class CustomBasedResourceController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new ObjectResult("result");
        }

        [AuthorizeWith(Permission = "custompermission")]
        [HttpGet]
        public IActionResult GetSingle()
        {
            return new ObjectResult("result");
        }

        [AuthorizeWith(Permissions = new[] { "permission1", "permission2" })]
        [HttpGet]
        public IActionResult GetMultiple()
        {
            return new ObjectResult("result");
        }
    }
}
