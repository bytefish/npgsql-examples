// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GitClub.Database.Models;
using System.Text.Json;

namespace GitClub.Infrastructure
{
    /// <summary>
    /// Static Methods to simplify working with a <see cref="OutboxEvent"/>.
    /// </summary>
    public static class OutboxEventUtils
    {
        /// <summary>
        /// Creates a new <see cref="OutboxEvent"/> from a given message Payload.
        /// </summary>
        /// <typeparam name="TMessageType">Type of the Message</typeparam>
        /// <param name="message">Message Payload</param>
        /// <param name="lastEditedBy">User that created the Outbox Event</param>
        /// <returns>An <see cref="OutboxEvent"/> that could be used</returns>
        public static OutboxEvent Create<TMessageType>(TMessageType message, int lastEditedBy)
        {
            var outboxEvent = new OutboxEvent
            {
                EventType = typeof(TMessageType).FullName!,
                Payload = JsonSerializer.SerializeToDocument(message),
                LastEditedBy = lastEditedBy
            };

            return outboxEvent;
        }

        /// <summary>
        /// Tries to get the deserialize the JSON Payload to the Type given in the 
        /// <see cref="OutboxEvent"/>. This returns an <see cref="object?"/>, so you 
        /// should do pattern matching on the consumer-side.
        /// </summary>
        /// <param name="outboxEvent">Outbox Event with typed Payload</param>
        /// <param name="result">The Payload deserialized to the Event Type</param>
        /// <returns><see cref="true"/>, if the payload can be deserialized; else <see cref="false"></returns>
        public static bool TryGetOutboxEventPayload(this OutboxEvent outboxEvent, out object? result)
        {
            result = null;

            // Maybe throw here? We should probably log it at least...
            var type = Type.GetType(outboxEvent.EventType, throwOnError: false);

            if (type == null)
            {
                return false;
            }

            result = JsonSerializer.Deserialize(outboxEvent.Payload, type);

            return true;
        }

    }
}