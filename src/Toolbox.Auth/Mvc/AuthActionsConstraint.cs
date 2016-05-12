using Microsoft.AspNet.Mvc.ActionConstraints;
using System;
using Toolbox.Auth.Options;

namespace Toolbox.Auth.Mvc
{
    public class AuthActionsConstraint : IActionConstraint
    {
        private readonly AuthOptions _authOptions;

        public AuthActionsConstraint(AuthOptions options)
        {
            _authOptions = options;
        }

        public int Order
        {
            get { return Int32.MaxValue; }
        }

        public bool Accept(ActionConstraintContext context)
        {
            return _authOptions.EnableCookieAuth;
        }
    }
}
