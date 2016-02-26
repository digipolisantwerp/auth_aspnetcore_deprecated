using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth;
using Toolbox.Auth.Authorization;

namespace SampleApp.Controllers.Api
{
    [Route("api/[Controller]")]
    //[AuthorizePermissions] // => apply convention based permissions on all controller actions
    public class TicketsController
    {
        //The purpose of this controller is to demonstrate the use of the "AuthorizePermissions" attribute using the default conventions
        //To demonstrate that the conventions are based on the http request method and not the action method name, the method names are appended with "Action"

        [HttpGet]
        [AuthorizeByConvention]  // => a user with permission 'read-tickets' will be allowed
        //or [Authorize(Policy = Policies.ConventionBased)]
        public IActionResult GetAction()
        {
            return new ObjectResult("Authorized: response from tickets GetAction()");
        }

        [HttpGet]
        [Route("getactionwithcustompermission")]
        [AuthorizeByConvention]
        [AuthorizeWith(CustomPermission = "permission-125")]  // => a user with permission 'permission-125' will be allowed
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
               
    }
}
