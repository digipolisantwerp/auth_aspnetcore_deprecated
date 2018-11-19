using Digipolis.Auth.Options;
using Digipolis.Auth.PDP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Digipolis.Auth.Controllers
{
    [Authorize(AuthenticationSchemes = AuthSchemes.JwtHeaderAuth)]
    public class PermissionsController : Controller
    {
        private readonly AuthOptions _authOptions;
        private readonly IPolicyDecisionProvider _policyDecisionProvider;

        public PermissionsController(IPolicyDecisionProvider policyDecisionProvider, IOptions<AuthOptions> options)
        {
            _policyDecisionProvider = policyDecisionProvider;
            _authOptions = options.Value;
        }

        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _policyDecisionProvider.GetPermissionsAsync(User.Identity.Name, _authOptions.ApplicationName);

            return Ok(permissions.permissions);
        }
    }
}
