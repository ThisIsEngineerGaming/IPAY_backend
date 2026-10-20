using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.UnitTests.Support
{
    public static class ControllerClaimsHelper
    {
        /// <summary>
        /// Like WithUser, but lets a test choose the exact claims (email, "sub", "nameid"...) the token carries.
        /// </summary>
        public static T WithClaims<T>(this T controller, params Claim[] claims) where T : ControllerBase
        {
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
