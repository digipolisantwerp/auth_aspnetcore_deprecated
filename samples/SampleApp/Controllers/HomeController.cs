using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using System.Net;
using Toolbox.Auth.Authorization;
using Toolbox.Auth;

namespace SampleApp.Controllers
{
    //[AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth, Permission = Constants.ApplicationLoginPermission)]
    //[Authorize(Policy = Constants.ApplicationUser)]
    [Authorize(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //[AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.TokenInCookie, Permission = Constants.ApplicationLoginPermission)]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [AllowAnonymous]
        //[Route("noaccess")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
