using Microsoft.AspNetCore.Mvc;

namespace Digipolis.Auth.UnitTests.Utilities
{
    public class TestController
    {
        public IActionResult Get()
        {
            return new OkResult();
        }
    }
}
