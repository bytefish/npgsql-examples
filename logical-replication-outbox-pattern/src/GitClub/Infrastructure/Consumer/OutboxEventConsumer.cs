// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GitClub.Database.Models;
using GitClub.Infrastructure.Logging;

namespace GitClub.Infrastructure.Consumer
{
    public class OutboxEventConsumer : IOutboxEventConsumer
    {
        private readonly ILogger<OutboxEventConsumer> _logger;

        public OutboxEventConsumer(ILogger<OutboxEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task ConsumeOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();


            if (outboxEvent.Payload == null)
            {
                _logger.LogWarning("Event doesn't contain a JSON Payload");

                return Task.CompletedTask;
            }

            var success = outboxEvent.TryGetOutboxEventPayload(out object? payload);

            // Maybe it's better to throw up, if we receive an event, we can't handle? But probably 
            // this wasn't meant for our Service at all? We don't know, so we log a Warning and go 
            // on with life ...
            if (!success)
            {
                _logger.LogWarning("Failed to get Data from OutboxEvent (Id = {OutboxEventId}, EventType = {OutboxEventType})", outboxEvent.Id, outboxEvent.EventType);

                return Task.CompletedTask;
            }

            _logger.LogInformation("Processing Event of Type {OutboxEventType} and Content {OutboxEventContent}", outboxEvent.EventType, outboxEvent.Payload.RootElement.ToString());

            return Task.CompletedTask;
        }
    }
}
