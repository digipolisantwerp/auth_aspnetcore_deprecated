using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Auth.Mvc;
using Toolbox.Auth.Options;
using Xunit;

namespace Toolbox.Auth.UnitTests.Mvc
{
    public class AuthActionsConstraintTests
    {
        [Fact]
        public void AcceptWhenCookieAuthIsEnabled()
        {
            var authOptions = new AuthOptions { EnableCookieAuth = true };
            var constraint = new AuthActionsConstraint(authOptions);

            var result = constraint.Accept(null);

            Assert.True(result);
        }

        [Fact]
        public void DenyWhenCookieAuthIsDisabled()
        {
            var authOptions = new AuthOptions { EnableCookieAuth = false };
            var constraint = new AuthActionsConstraint(authOptions);

            var result = constraint.Accept(null);

            Assert.False(result);
        }
    }
}
