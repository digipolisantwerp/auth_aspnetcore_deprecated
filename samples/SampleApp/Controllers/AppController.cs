using Digipolis.Auth;
using Digipolis.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Controllers
{
    [AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth, Permission = Constants.ApplicationLoginPermission)]
    //[Authorize(Policy = Constants.ApplicationUser)]
    //[Authorize(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth)]
    //[Authorize]
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
