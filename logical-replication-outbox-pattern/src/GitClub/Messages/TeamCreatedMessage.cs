// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GitClub.Messages
{
    public class TeamCreatedMessage
    {
        /// <summary>
        /// Gets or sets the Team ID.
        /// </summary>
        [JsonPropertyName("teamId")]
        public required int TeamId { get; set; }

        /// <summary>
        /// Gets or sets the Organization ID.
        /// </summary>
        [JsonPropertyName("organizationId")]
        public required int OrganizationId { get; set; }
    }
}
