// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GitClub.Messages
{
    public class OrganizationDeletedMessage
    {
        /// <summary>
        /// Gets or sets the Organization ID.
        /// </summary>
        [JsonPropertyName("organizationId")]
        public required int OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the associated Teams deleted.
        /// </summary>
        [JsonPropertyName("deletedTeams")]
        public List<TeamDeletedMessage> DeletedTeams { get; set; } = [];

        /// <summary>
        /// Gets or sets the associated Repositories deleted.
        /// </summary>
        [JsonPropertyName("deletedRepositories")]
        public List<RepositoryDeletedMessage> DeletedRepositories { get; set; } = [];

        /// <summary>
        /// Gets or sets the associated Users deleted.
        /// </summary>
        [JsonPropertyName("deletedRepositories")]
        public List<UserDeletedMessage> DeletedUsers { get; set; } = [];
    }
}
