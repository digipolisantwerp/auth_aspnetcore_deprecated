using Microsoft.AspNetCore.Mvc;
using Digipolis.Auth.Authorization;

namespace Digipolis.Auth.UnitTests.Authorization.ResolverTests
{
    public class ConventionsBasedResourceController : Controller
    {
        [AuthorizeByConvention]
        [HttpGet]
        public IActionResult Get()
        {
            return new ObjectResult("result");
        }

        [AuthorizeByConvention]
        [HttpPost]
        public IActionResult Post()
        {
            return new ObjectResult("result");
        }

        [AuthorizeByConvention]
        [HttpPut]
        public IActionResult Put()
        {
            return new ObjectResult("result");
        }

        [AuthorizeByConvention]
        [HttpPatch]
        public IActionResult Patch()
        {
            return new ObjectResult("result");
        }

        [AuthorizeByConvention]
        [HttpDelete]
        public IActionResult Delete()
        {
            return new ObjectResult("result");
        }
    }
}
