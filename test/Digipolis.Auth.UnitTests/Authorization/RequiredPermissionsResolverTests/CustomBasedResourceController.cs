using Microsoft.AspNetCore.Mvc;
using Digipolis.Auth.Authorization;

namespace Digipolis.Auth.UnitTests.Authorization.ResolverTests
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
