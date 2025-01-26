// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using Npgsql.Replication.PgOutput.Messages;
using Npgsql.Replication;
using System.Runtime.CompilerServices;
using System.Text.Json;
using GitClub.Infrastructure.Logging;
using GitClub.Database.Models;
using Npgsql.Replication.PgOutput;

namespace GitClub.Infrastructure.Postgres
{
    public class PostgresOutboxSubscriber
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Options to configure the Wal Receiver.
        /// </summary>
        private readonly PostgresOutboxSubscriberOptions _options;

        /// <summary>
        /// Creates a new <see cref="PostgresReplicationClient" />.
        /// </summary>
        /// <param name="logger">Logger to log messages</param>
        /// <param name="options">Options to configure the service</param>
        public PostgresOutboxSubscriber(ILogger logger, IOptions<PostgresOutboxSubscriberOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Instructs the server to start the Logical Streaming Replication Protocol (pgoutput logical decoding 
        /// plugin), starting at WAL location walLocation or at the slot's consistent point if walLocation isn't 
        /// specified. The server can reply with an error, for example if the requested section of the WAL has 
        /// already been recycled.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token to stop the Logical Replication</param>
        /// <returns>Replication Transactions</returns>
        /// <exception cref="InvalidOperationException">Thrown when a replication message can't be handled</exception>
        public async IAsyncEnumerable<OutboxEvent> StartOutboxEventStreamAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // This is the only way to create the Replication Connection, I have found no 
            // way to utilize the NpgsqlDataSource for it. There might be a way though.
            var replicationConnection = new LogicalReplicationConnection(_options.ConnectionString);

            // Open the Connection.
            await replicationConnection
                .Open(cancellationToken)
                .ConfigureAwait(false);

            // Reference to the Publication.
            var replicationPublication = new PgOutputReplicationOptions(_options.PublicationName, protocolVersion: PgOutputProtocolVersion.V3, binary: true, streamingMode: PgOutputStreamingMode.Off);

            // Reference to the Replication Slot.
            var replicationSlot = new PgOutputReplicationSlot(_options.ReplicationSlotName);

            await foreach (var message in replicationConnection
                .StartReplication(replicationSlot, replicationPublication, cancellationToken)
                .ConfigureAwait(false))
            {
                _logger.LogDebug("Received Postgres WAL Message (Type = {WalMessageType}, ServerClock = {WalServerClock}, WalStart = {WalStart}, WalEnd = {WalEnd})",
                    message.GetType().Name, message.ServerClock, message.WalStart, message.WalEnd);

                if (message is InsertMessage insertMessage)
                {
                    if (IsOutboxTable(insertMessage))
                    {
                        var outboxEvent = await ConvertToOutboxEventAsync(insertMessage, cancellationToken).ConfigureAwait(false);

                        yield return outboxEvent;
                    }
                }

                // Acknowledge the message. This should probably depend on wether a Transaction has finally been acknowledged
                // or not and is going to be something for future implementations.
                replicationConnection.SetReplicationStatus(message.WalEnd);
            }
        }

        bool IsOutboxTable(InsertMessage message)
        {
            return string.Equals(_options.OutboxEventSchemaName, message.Relation.Namespace, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(_options.OutboxEventTableName, message.Relation.RelationName, StringComparison.InvariantCultureIgnoreCase);
        }

        async ValueTask<OutboxEvent> ConvertToOutboxEventAsync(InsertMessage insertMessage, CancellationToken cancellationToken)
        {
            var values = await ConvertToDictionaryAsync(insertMessage, cancellationToken).ConfigureAwait(false);

            return ConvertToOutboxEventAsync(values);
        }

        async ValueTask<Dictionary<string, object?>> ConvertToDictionaryAsync(InsertMessage insertMessage, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = new Dictionary<string, object?>();

            int columnIdx = 0;

            await foreach (var replicationValue in insertMessage.NewRow)
            {
                var columnName = insertMessage.Relation
                    .Columns[columnIdx++].ColumnName;
                result[columnName] = await GetValue(replicationValue, cancellationToken);
            }

            return result;
        }

        async Task<object?> GetValue(ReplicationValue replicationValue, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var value = await replicationValue
                .Get(cancellationToken)
                .ConfigureAwait(false);

            if (replicationValue.IsDBNull)
            {
                return null;
            }

            return value;
        }

        OutboxEvent ConvertToOutboxEventAsync(Dictionary<string, object?> values)
        {
            _logger.TraceMethodEntry();

            var payload = GetRequiredValue<string>(values, "payload");

            var outboxEvent = new OutboxEvent
            {
                Id = GetRequiredValue<int>(values, "outbox_event_id"),
                CorrelationId1 = GetOptionalValue<string>(values, "correlation_id_1"),
                CorrelationId2 = GetOptionalValue<string>(values, "correlation_id_2"),
                CorrelationId3 = GetOptionalValue<string>(values, "correlation_id_3"),
                CorrelationId4 = GetOptionalValue<string>(values, "correlation_id_4"),
                EventType = GetRequiredValue<string>(values, "event_type"),
                EventSource = GetRequiredValue<string>(values, "event_source"),
                EventTime = GetRequiredValue<DateTime>(values, "event_time"),
                Payload = JsonSerializer.Deserialize<JsonDocument>(payload)!,
                LastEditedBy = GetRequiredValue<int>(values, "last_edited_by")
            };

            return outboxEvent;
        }

        T GetRequiredValue<T>(Dictionary<string, object?> values, string key)
        {
            if (!values.ContainsKey(key))
            {
                throw new InvalidOperationException($"Value is required for key '{key}'");
            }

            if (values[key] is not T t)
            {
                throw new InvalidOperationException($"Value is not Type '{typeof(T).Name}'");
            }

            return t;
        }

        T? GetOptionalValue<T>(Dictionary<string, object?> values, string key, T? defaultValue = default)
        {
            if (!values.ContainsKey(key))
            {
                return defaultValue;
            }

            if (values[key] is T t)
            {
                return t;
            }

            return defaultValue;
        }
    }
}
