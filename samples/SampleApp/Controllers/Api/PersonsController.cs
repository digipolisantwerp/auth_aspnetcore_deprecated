using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Digipolis.Auth;

namespace SampleApp.Controllers.Api
{
    //The purpose of this controller is to demonstrate the use of the default Microsoft.AspNet Authorization attributes


    [Authorize(AuthenticationSchemes = AuthSchemes.JwtHeaderAuth)] // => Only authenticated users are allowed to access the resources on this controller
    [Route("api/[Controller]")]
    public class PersonsController
    {
        [HttpGet]                   
        public IActionResult GetAction()
        {
            return new ObjectResult("Authorized: response from persons GetAction()");
        }

        // User with role "PersonAdministrator" is allowed. 
        // To test this you can add a claim with value: PersonAdministrator and claimtype: http://schemas.microsoft.com/ws/2008/06/identity/claims/role to the jwt token.
        [HttpPost]
        [Authorize(Roles = Constants.PersonAdministrator, AuthenticationSchemes = AuthSchemes.JwtHeaderAuth)] 
        public IActionResult PostAction()
        {
            return new ObjectResult("Authorized: response from persons PostAction()");
        }

        // The criteria for the policy named UserWithCustomClaimOnly has to meet (a claim of type 'custom' has to be present).
        // See the PolicyBuilder.cs where the policies are defined and Startup.cs where they are set.
        // To test this add a claim of type 'Custom' to the jwt token.
        [HttpPut]
        [Authorize(Policy = Constants.UserWithCustomClaimOnly)] 
        public IActionResult PutAction()
        {
            return new ObjectResult("Authorized: response from persons PutAction()");
        }

        [HttpDelete]
        public IActionResult DeleteAction()
        {
            return new ObjectResult("Authorized: response from persons DeleteAction()");
        }

        [HttpGet]
        [AllowAnonymous]        // => Overrides the Authorize attribute
        [Route("allowanonymous")]
        public IActionResult AllowAnonymous()
        {
            return new ObjectResult("Authorized: response from persons AllowAnonymous()");
        }
    }
}
