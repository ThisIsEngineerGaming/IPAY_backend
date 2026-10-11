using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    /// <summary>
    /// A do-nothing endpoint the client pings while someone is logged in. The JWT bearer check in Program.cs
    /// rejects a token whose role no longer matches the user's current role (401), so this is how the site
    /// notices a role change within seconds instead of waiting for the person's next real request.
    /// </summary>
    [ApiController]
    [Route("api/session")]
    public class SessionController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Get() => NoContent();
    }
}
