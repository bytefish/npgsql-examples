// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace GitClub.Messages
{
    /// <summary>
    /// A User has been deleted and all assignments need to be terminated.
    /// </summary>
    public class UserDeletedMessage
    {
        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
    }
}
