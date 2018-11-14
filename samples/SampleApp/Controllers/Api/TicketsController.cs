using Digipolis.Auth.Authorization;
using Digipolis.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Controllers.Api
{
    [Route("api/[Controller]")]
    //[AuthorizeByConvention] // => apply convention based permissions on all controller actions
    public class TicketsController : Controller
    {
        private readonly IAuthService _authService;

        //The purpose of this controller is to demonstrate the use of the "AuthorizeByConvention" and "AuthorizeWith" attributes.
        public TicketsController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]                   // => The Http method determines the first part of the permission, not the action method name!
        [AuthorizeByConvention]     // => a user with permission 'read-tickets' will be allowed
        //[Authorize(Policy = Policies.ConventionBased)]  // => results in the same as using the "AuthorizeByConvention" attribute
        //or [Authorize(Policy = Policies.ConventionBased)]
        public IActionResult GetAction()
        {
            var user = _authService.User;
            var userToken = _authService.UserToken;
            return new ObjectResult($"Authorized: response from tickets GetAction(). User: {user.Identity.Name}");
        }

        [HttpGet]
        [Route("getactionwithcustompermission")]
        [AuthorizeWith(Permission = "permission-125")]  // => Two attributes placed : only a user with permission 'read-tickets' AND 'permission-125' will be allowed
        //[AuthorizeWith(Permissions = new[] { "permission-125", "permission-321" })] // or at least one of the provided permissions
        public IActionResult GetActionWithCustomPermission()
        {
            return new ObjectResult("Authorized: response from tickets GetActionWithCustomPermission()");
        }

        [HttpPost]
        [AuthorizeByConvention]  // => a user with permission 'create-tickets' will be allowed
        public IActionResult PostAction()
        {
            return new ObjectResult("Authorized: response from tickets PostAction()");
        }

        [HttpPut]
        [AuthorizeByConvention]  // => a user with permission 'update-tickets' will be allowed
        public IActionResult PutAction()
        {
            return new ObjectResult("Authorized: response from tickets PutAction()");
        }

        [HttpDelete]
        [AuthorizeByConvention]  // => a user with permission 'delete-tickets' will be allowed
        public IActionResult DeleteAction()
        {
            return new ObjectResult("Authorized: response from tickets DeleteAction()");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("allowanonymous")]
        public IActionResult AllowAnonymous()
        {
            return new ObjectResult("Authorized: response from tickets AllowAnonymous()");
        }

    }
}
