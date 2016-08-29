using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;

namespace Digipolis.Auth.Controllers
{
    //[Authorize(ActiveAuthenticationSchemes = AuthSchemes.JwtHeaderAuth)]
    public class PermissionsController : Controller
    {
        private readonly AuthOptions _authOptions;
        private readonly IPolicyDescisionProvider _policyDescisionProvider;

        public PermissionsController(IPolicyDescisionProvider policyDescisionProvider, IOptions<AuthOptions> options)
        {
            _policyDescisionProvider = policyDescisionProvider;
            _authOptions = options.Value;
        }

        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _policyDescisionProvider.GetPermissionsAsync(User.Identity.Name, _authOptions.ApplicationName);

            return Ok(permissions.permissions);
        }
    }
}
