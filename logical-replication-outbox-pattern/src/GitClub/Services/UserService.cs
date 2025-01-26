// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using GitClub.Database;
using GitClub.Database.Models;
using GitClub.Infrastructure;
using GitClub.Infrastructure.Authentication;
using GitClub.Infrastructure.Constants;
using GitClub.Infrastructure.Logging;
using GitClub.Messages;
using System.Security.Claims;

namespace GitClub.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UserService(ILogger<UserService> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<User> CreateUserAsync(User user, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            bool isAuthorized = currentUser.IsInRole(Roles.Administrator);

            if (!isAuthorized)
            {
                throw new Exception("Insufficient Permissions to create a new user");
            }

            using ApplicationDbContext applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            user.LastEditedBy = currentUser.UserId;

            await applicationDbContext
                .AddAsync(user, cancellationToken)
                .ConfigureAwait(false);

            await applicationDbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return user;
        }

        public async Task DeleteUserByUserIdAsync(int userId, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();


            if (currentUser.UserId == userId)
            {
                throw new Exception("Cannot delete own user");
            }

            using ApplicationDbContext applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            // Now we can safely update and delete the data in a transaction
            using (var transaction = await applicationDbContext.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false))
            {
                await applicationDbContext.Organizations
                    .Where(x => x.LastEditedBy == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastEditedBy, Users.GhostUserId));

                await applicationDbContext.Repositories
                    .Where(x => x.LastEditedBy == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastEditedBy, Users.GhostUserId));

                await applicationDbContext.Teams
                    .Where(x => x.LastEditedBy == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastEditedBy, Users.GhostUserId));

                await applicationDbContext.Users
                    .Where(x => x.LastEditedBy == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastEditedBy, Users.GhostUserId));

                await applicationDbContext.Issues
                    .Where(x => x.LastEditedBy == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastEditedBy, Users.GhostUserId));

                // We also need to assign the Issue Creator to the GhostUser:
                await applicationDbContext.Issues
                    .Where(x => x.CreatedBy == userId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.LastEditedBy, Users.GhostUserId)
                        .SetProperty(p => p.CreatedBy, Users.GhostUserId));

                UserDeletedMessage userDeletedMessage = new UserDeletedMessage
                {
                    UserId = userId,
                };

                OutboxEvent outboxEvent = OutboxEventUtils.Create(userDeletedMessage, currentUser);

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

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            User? user = await applicationDbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                throw new Exception("Authentication failed");
            }

            return user;
        }

        public async Task<List<Claim>> GetClaimsAsync(string email, string[] roles, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using ApplicationDbContext applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            User? user = await applicationDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                throw new Exception("Authentication failed");
            }

            // Build the Claims for the ClaimsPrincipal
            List<Claim> claims = CreateClaims(user, roles);

            return claims;
        }

        private List<Claim> CreateClaims(User user, string[] roles)
        {
            _logger.TraceMethodEntry();

            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Sid, Convert.ToString(user.Id)),
                new Claim(ClaimTypes.Name, Convert.ToString(user.PreferredName)),
            ];

            // Roles:
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}