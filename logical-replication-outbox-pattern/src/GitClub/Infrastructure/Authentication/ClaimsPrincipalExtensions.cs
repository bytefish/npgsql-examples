// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;

namespace GitClub.Infrastructure.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.Sid);

            if (userId == null)
            {
                throw new InvalidOperationException("No UserID found for ClaimsPrincipal");
            }

            if (!int.TryParse(userId, out var result))
            {
                throw new InvalidOperationException("UserID could not be converted to an Int32");
            }

            return result;
        }
    }
}
