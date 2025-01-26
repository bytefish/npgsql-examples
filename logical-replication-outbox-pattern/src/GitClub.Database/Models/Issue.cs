// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace GitClub.Database.Models
{
    public class Issue : Entity
    {
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the Closed flag.
        /// </summary>
        public required bool Closed { get; set; }

        /// <summary>
        /// Gets or sets the RepositoryId.
        /// </summary>
        public int RepositoryId { get; set; }

        /// <summary>
        /// Gets or sets the CreatedBy.
        /// </summary>
        public int CreatedBy { get; set; }
    }
}
