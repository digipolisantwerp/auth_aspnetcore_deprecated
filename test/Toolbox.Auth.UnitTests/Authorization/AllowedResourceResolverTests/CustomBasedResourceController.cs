using Microsoft.AspNet.Mvc;
using Toolbox.Auth.Authorization;

namespace Toolbox.Auth.UnitTests.Authorization.AllowedResourceResolverTests
{
    public class CustomBasedResourceController : Controller
    {
        [AuthorizeWith(CustomPermission = "custompermission")]
        [HttpGet]
        public IActionResult GetSingle()
        {
            return new ObjectResult("result");
        }

        [AuthorizeWith(CustomPermissions = new[] { "permission1", "permission2" })]
        [HttpGet]
        public IActionResult GetMultiple()
        {
            return new ObjectResult("result");
        }
    }
}
