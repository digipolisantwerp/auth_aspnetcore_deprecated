using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Digipolis.Auth.Authorization
{
    public class SignalRBasedAuthorizationHandler : AuthorizationHandler<SignalRBasedRequirements>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public SignalRBasedAuthorizationHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SignalRBasedRequirements requirement)
        {
            if (!string.IsNullOrWhiteSpace(_contextAccessor.HttpContext?.Request?.Headers?["Authorization"]) ||
                ((_contextAccessor.HttpContext?.Request?.Query?.TryGetValue("access_token", out var accessToken) ?? false) && !string.IsNullOrWhiteSpace(accessToken)))
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
