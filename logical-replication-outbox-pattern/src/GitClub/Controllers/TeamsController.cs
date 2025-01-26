// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GitClub.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GitClub.Database.Models;
using GitClub.Infrastructure.Authentication;
using GitClub.Infrastructure.Logging;
using GitClub.Services;

namespace GitClub.Controllers
{
    [Route("[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(ILogger<TeamsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeam([FromServices] TeamService teamService, [FromServices] CurrentUser currentUser, [FromRoute(Name = "id")] int id, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            Team team = await teamService.GetTeamByIdAsync(id, currentUser, cancellationToken);

            return Ok(team);
        }

        [HttpGet]
        [Authorize(Policy = Policies.RequireUserRole)]
        [EnableRateLimiting(Policies.PerUserRatelimit)]
        public async Task<IActionResult> GetTeams([FromServices] TeamService teamService, [FromServices] CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            List<Team> teams = await teamService.GetTeamsAsync(currentUser, cancellationToken);

            return Ok(teams);
        }

        [HttpPost]
        [Authorize(Policy = Policies.RequireUserRole)]
        [EnableRateLimiting(Policies.PerUserRatelimit)]
        public async Task<IActionResult> PostTeam([FromServices] TeamService teamService, [FromServices] CurrentUser currentUser, [FromBody] Team team, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            Team createdTeam = await teamService.CreateTeamAsync(team, currentUser, cancellationToken);

            return Ok(createdTeam);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.RequireUserRole)]
        [EnableRateLimiting(Policies.PerUserRatelimit)]
        public async Task<IActionResult> PutTeam([FromServices] TeamService teamService, [FromServices] CurrentUser currentUser, [FromRoute(Name = "id")] int id, [FromBody] Team team, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            Team updatedTeam = await teamService.UpdateTeamAsync(id, team, currentUser, cancellationToken);

            return Ok(updatedTeam);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.RequireUserRole)]
        [EnableRateLimiting(Policies.PerUserRatelimit)]
        public async Task<IActionResult> DeleteTeam([FromServices] TeamService teamService, [FromServices] CurrentUser currentUser, [FromRoute(Name = "id")] int key, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await teamService.DeleteTeamAsync(key, currentUser, cancellationToken);

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}