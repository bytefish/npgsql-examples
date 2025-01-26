// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using GitClub.Database;
using GitClub.Database.Models;
using GitClub.Infrastructure;
using GitClub.Infrastructure.Authentication;
using GitClub.Infrastructure.Logging;
using GitClub.Messages;

namespace GitClub.Services
{
    public class TeamService
    {
        private readonly ILogger<TeamService> _logger;

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public TeamService(ILogger<TeamService> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Team> CreateTeamAsync(Team team, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            // Make sure the Current User is the last editor:
            team.LastEditedBy = currentUser.UserId;

            // Add the new Team
            await applicationDbContext
                .AddAsync(team, cancellationToken)
                .ConfigureAwait(false);

            var outboxEvent = OutboxEventUtils.Create(new TeamCreatedMessage
            {
                OrganizationId = team.OrganizationId,
                TeamId = team.Id,
            }, lastEditedBy: currentUser.UserId);

            await applicationDbContext
                .AddAsync(outboxEvent, cancellationToken)
                .ConfigureAwait(false);

            await applicationDbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return team;
        }

        public async Task<Team> GetTeamByIdAsync(int teamId, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var team = await applicationDbContext.Teams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken)
                .ConfigureAwait(false);

            if (team == null)
            {
                throw new Exception($"Entity '{teamId}' not found");
            }

            return team;
        }

        public async Task<List<Team>> GetTeamsAsync(CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var teams = await applicationDbContext.Teams
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return teams;
        }

        public async Task<List<Team>> GetTeamsByOrganizationIdAsync(int organizationId, int userId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var teams = await applicationDbContext.Teams
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return teams;
        }

        public async Task<Team> UpdateTeamAsync(int teamId, Team values, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var original = await applicationDbContext.Teams.AsNoTracking()
                .Where(x => x.Id == teamId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (original == null)
            {
                throw new Exception($"Entity '{teamId}' not found");
            }

            using (var transaction = await applicationDbContext.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                int rowsAffected = await applicationDbContext.Teams
                    .Where(t => t.Id == teamId && t.RowVersion == values.RowVersion)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.Name, values.Name)
                        .SetProperty(x => x.LastEditedBy, currentUser.UserId), cancellationToken)
                    .ConfigureAwait(false);

                if (rowsAffected == 0)
                {
                    throw new Exception("Concurrency Exception");
                }

                var outboxEvent = OutboxEventUtils.Create(new TeamUpdatedMessage { TeamId = teamId }, lastEditedBy: currentUser.UserId);

                await applicationDbContext.OutboxEvents
                    .AddAsync(outboxEvent, cancellationToken)
                    .ConfigureAwait(false);

                await applicationDbContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                await transaction
                    .CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            var updated = await applicationDbContext.Teams.AsNoTracking()
                .Where(x => x.Id == teamId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (updated == null)
            {
                throw new Exception($"Entity '{teamId}' not found");
            }

            return updated;
        }

        public async Task DeleteTeamAsync(int teamId, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();
            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var team = await applicationDbContext.Teams.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken)
                .ConfigureAwait(false);

            if (team == null)
            {
                throw new Exception($"Entity '{teamId}' not found");
            }


            using (var transaction = await applicationDbContext.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                await applicationDbContext.Teams
                        .Where(t => t.Id == team.Id)
                        .ExecuteDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);

                // Write Outbox Event at the same time
                var outboxEvent = OutboxEventUtils.Create(new TeamDeletedMessage
                {
                    TeamId = team.Id,
                }, lastEditedBy: currentUser.UserId);

                await applicationDbContext
                    .AddAsync(outboxEvent, cancellationToken)
                    .ConfigureAwait(false);

                await applicationDbContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                await transaction
                    .CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

    }
}
