using Microsoft.AspNetCore.Mvc;

namespace Toolbox.Auth.UnitTests.Utilities
{
    public class TestController
    {
        public IActionResult Get()
        {
            return new OkResult();
        }
    }
}
