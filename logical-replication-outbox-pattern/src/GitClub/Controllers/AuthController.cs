// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using GitClub.Infrastructure.Logging;
using GitClub.Models;
using GitClub.Services;
using System.Security.Claims;

namespace GitClub.Controllers
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromServices] UserService userService, [FromBody] Credentials credentials, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // create a claim for each request scope
            List<Claim> userClaims = await userService
                .GetClaimsAsync(credentials.Email, credentials.Roles, cancellationToken)
                .ConfigureAwait(false);

            // Create the ClaimsPrincipal
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // It's a valid ClaimsPrincipal, sign in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { IsPersistent = credentials.RememberMe });

            return Ok();

        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.TraceMethodEntry();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }
    }
}