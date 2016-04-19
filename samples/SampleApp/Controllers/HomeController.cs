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
    [AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.TokenInCookie, Permission = Constants.ApplicationLoginPermission)]
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
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
