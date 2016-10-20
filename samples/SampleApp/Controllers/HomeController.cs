using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Digipolis.Auth.Authorization;
using Digipolis.Auth;
using Digipolis.Auth.Jwt;
using Digipolis.Auth.Services;

namespace SampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenRefreshAgent _tokenRefreshAgent;

        public HomeController(ITokenRefreshAgent tokenRefreshAgent)
        {
            _tokenRefreshAgent = tokenRefreshAgent;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> SignOut([FromServices] IAuthService authService)
        {
            var redirectUrl = await authService.LogOutAsync(ControllerContext, "Home", "Index");

            return Redirect(redirectUrl);
        }
    }
}
