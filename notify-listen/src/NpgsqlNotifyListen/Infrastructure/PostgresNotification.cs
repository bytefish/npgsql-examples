// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NpgsqlNotifyListen.Infrastructure
{
    /// <summary>
    /// A Notification received from Postgres NOTIFY / LISTEN.
    /// </summary>
    public record PostgresNotification
    {
        /// <summary>
        /// Gets or sets the PID.
        /// </summary>
        public required int PID { get; set; }

        /// <summary>
        /// Gets or sets the Channel.
        /// </summary>
        public required string Channel { get; set; }

        /// <summary>
        /// Gets or sets the Payload.
        /// </summary>
        public required string Payload { get; set; }
    }
}
