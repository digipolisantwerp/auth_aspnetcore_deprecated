using Microsoft.AspNet.Mvc;
using Toolbox.Auth.Authorization;

namespace Toolbox.Auth.UnitTests.Authorization.ResolverTests
{
    public class CustomBasedResourceController : Controller
    {
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
