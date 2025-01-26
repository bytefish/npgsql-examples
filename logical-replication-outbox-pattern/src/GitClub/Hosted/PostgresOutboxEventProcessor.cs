// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using GitClub.Infrastructure.Consumer;
using GitClub.Infrastructure.Logging;
using GitClub.Infrastructure.Postgres;

namespace GitClub.Hosted
{
    /// <summary>
    /// Options to configure the OutboxEvent Processor.
    /// </summary>
    public class PostgresOutboxEventProcessorOptions
    {
        /// <summary>
        /// Gets or sets the ConnectionString for the Replication Stream.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the PublicationName the Service is listening to.
        /// </summary>
        public required string PublicationName { get; set; }

        /// <summary>
        /// Gets or sets the ReplicationSlot the Service is listening to.
        /// </summary>
        public required string ReplicationSlotName { get; set; }

        /// <summary>
        /// Gets or sets the Table the Outbox Events are written to.
        /// </summary>
        public required string OutboxEventTableName { get; set; }

        /// <summary>
        /// Gets or sets the Schema the Outbox Events are written to.
        /// </summary>
        public required string OutboxEventSchemaName { get; set; }
    }

    /// <summary>
    /// Processes Outbox Events.
    /// </summary>
    public class PostgresOutboxEventProcessor : BackgroundService
    {
        private readonly ILogger<PostgresOutboxEventProcessor> _logger;

        private readonly PostgresOutboxEventProcessorOptions _options;
        private readonly IOutboxEventConsumer _outboxEventConsumer;

        public PostgresOutboxEventProcessor(ILogger<PostgresOutboxEventProcessor> logger, IOptions<PostgresOutboxEventProcessorOptions> options, IOutboxEventConsumer outboxEventConsumer)
        {
            _logger = logger;
            _options = options.Value;
            _outboxEventConsumer = outboxEventConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var outboxSubscriberOptions = new PostgresOutboxSubscriberOptions
            {
                ConnectionString = _options.ConnectionString,
                PublicationName = _options.PublicationName,
                ReplicationSlotName = _options.ReplicationSlotName,
                OutboxEventSchemaName = _options.OutboxEventSchemaName,
                OutboxEventTableName = _options.OutboxEventTableName
            };

            var outboxEventStream = new PostgresOutboxSubscriber(_logger, Options.Create(outboxSubscriberOptions));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var outboxEvent in outboxEventStream.StartOutboxEventStreamAsync(cancellationToken))
                    {
                        _logger.LogInformation("Processing OutboxEvent (Id = {OutboxEventId})", outboxEvent.Id);

                        try
                        {
                            await _outboxEventConsumer
                                .ConsumeOutboxEventAsync(outboxEvent, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to handle the OutboxEvent due to an Exception (ID = {OutboxEventId})", outboxEvent.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Logical Replication failed with an Error. Restarting the Stream.");

                    // Probably add some better Retry options ...
                    await Task
                        .Delay(200)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}