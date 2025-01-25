// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NpgsqlNotifyListen.Models
{
    public class Team : Entity
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the OrganizationId.
        /// </summary>
        public int OrganizationId { get; set; }
    }
}