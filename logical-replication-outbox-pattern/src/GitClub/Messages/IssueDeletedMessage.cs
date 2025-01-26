// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GitClub.Messages
{
    public class IssueDeletedMessage
    {
        /// <summary>
        /// Gets or sets the Issue ID.
        /// </summary>
        [JsonPropertyName("issueId")]
        public required int IssueId { get; set; }

        /// <summary>
        /// Gets or sets the Repository ID.
        /// </summary>
        [JsonPropertyName("repositoryId")]
        public required int RepositoryId { get; set; }
    }
}
