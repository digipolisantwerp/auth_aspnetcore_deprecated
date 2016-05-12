using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Options;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = AuthSchemes.JwtHeaderAuth)]
    public class PermissionsController : Controller
    {
        private readonly AuthOptions _authOptions;
        private readonly IPolicyDescisionProvider _policyDescisionProvider;

        public PermissionsController(IPolicyDescisionProvider policyDescisionProvider, IOptions<AuthOptions> options)
        {
            _policyDescisionProvider = policyDescisionProvider;
            _authOptions = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _policyDescisionProvider.GetPermissionsAsync(User.Identity.Name, _authOptions.ApplicationName);

            return Ok(permissions.permissions);
        }
    }
}
