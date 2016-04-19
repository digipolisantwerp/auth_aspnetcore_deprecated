using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.OptionsModel;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.PDP
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
