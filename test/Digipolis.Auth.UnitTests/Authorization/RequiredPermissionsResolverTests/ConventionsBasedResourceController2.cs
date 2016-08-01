using Microsoft.AspNetCore.Mvc;
using Digipolis.Auth.Authorization;

namespace Digipolis.Auth.UnitTests.Authorization.ResolverTests
{
    //[AuthorizeByConvention]
    public class ConventionsBasedResourceController2 : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new ObjectResult("result");
        }

        [HttpPost]
        public IActionResult Post()
        {
            return new ObjectResult("result");
        }

        [HttpPut]
        public IActionResult Put()
        {
            return new ObjectResult("result");
        }

        [HttpPatch]
        public IActionResult Patch()
        {
            return new ObjectResult("result");
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            return new ObjectResult("result");
        }
    }
}
