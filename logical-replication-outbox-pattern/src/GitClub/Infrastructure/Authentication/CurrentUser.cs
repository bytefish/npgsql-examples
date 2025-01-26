// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GitClub.Database.Models;
using System.Security.Claims;

namespace GitClub.Infrastructure.Authentication
{
    /// <summary>
    /// A Scoped Service to provide the current user information.
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// Gets or sets the User.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public ClaimsPrincipal Principal { get; set; } = default!;

        /// <summary>
        /// Gets the UserID for the current <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public int UserId => Principal.GetUserId();

        /// <summary>
        /// Checks if the User is Administrator
        /// </summary>
        public bool IsInRole(string role)
        {
            return Principal.IsInRole(role);
        }
    }
}
