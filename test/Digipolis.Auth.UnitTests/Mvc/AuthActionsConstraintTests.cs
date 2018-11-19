using Digipolis.Auth.Mvc;
using Digipolis.Auth.Options;
using Xunit;

namespace Digipolis.Auth.UnitTests.Mvc
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
