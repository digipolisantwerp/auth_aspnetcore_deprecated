using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Digipolis.Auth.Options;

namespace Digipolis.Auth.PDP
{
    //[Authorize(ActiveAuthenticationSchemes = AuthSchemes.TokenInHeader)]
    [Authorization.AuthorizeWith(Permission = "permission-125")]
    [Route("api/[controller]")]
    public class PermissionsController : Controller
    {
        private readonly AuthOptions _authOptions;
        private readonly IPolicyDescisionProvider _policyDescisionProvider;

        public PermissionsController(IOptions<AuthOptions> options, IPolicyDescisionProvider policyDescisionProvider)
        {
            _authOptions = options.Value;
            _policyDescisionProvider = policyDescisionProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pdpResponse = await _policyDescisionProvider.GetPermissionsAsync(User.Identity.Name, _authOptions.ApplicationName);

            return Ok(pdpResponse.permissions);
        }
    }
}
