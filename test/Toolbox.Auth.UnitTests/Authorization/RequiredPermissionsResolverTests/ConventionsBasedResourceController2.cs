using Microsoft.AspNet.Mvc;
using Toolbox.Auth.Authorization;

namespace Toolbox.Auth.UnitTests.Authorization.ResolverTests
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
