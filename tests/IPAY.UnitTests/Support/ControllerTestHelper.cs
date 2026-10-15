using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.UnitTests.Support
{
    public static class ControllerTestHelper
    {
        /// <summary>
        /// Gives a controller a signed-in user (id + roles), like a validated JWT would.
        /// Without a ControllerContext, ControllerBase.User is null and anything touching it throws.
        /// Pass id = null for a token that has no NameIdentifier claim.
        /// </summary>
        public static T WithUser<T>(this T controller, int? id, params string[] roles) where T : ControllerBase
        {
            var claims = new List<Claim>();
            if (id is not null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
                }
            };
            return controller;
        }
    }
}
