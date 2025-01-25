// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Threading.Channels;

namespace NpgsqlNotifyListen.Infrastructure
{
    /// <summary>
    /// This Service waits for Notifications received on a given Postgres Channel name.
    /// </summary>
    public class PostgresNotificationService : BackgroundService
    {
        private readonly ILogger<PostgresNotificationService> _logger;

        private readonly PostgresNotificationServiceOptions _options;
        private readonly IPostgresNotificationHandler _postgresNotificationHandler;
        private readonly NpgsqlDataSource _npgsqlDataSource;

        public PostgresNotificationService(ILogger<PostgresNotificationService> logger, IOptions<PostgresNotificationServiceOptions> options, NpgsqlDataSource npgsqlDataSource, IPostgresNotificationHandler postgresNotificationHandler)
        {
            _logger = logger;
            _options = options.Value;
            _npgsqlDataSource = npgsqlDataSource;
            _postgresNotificationHandler = postgresNotificationHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Now this could be a horrible thing to do when we experience backpressure... 
            var channel = Channel.CreateBounded<PostgresNotification>(new BoundedChannelOptions(_options.MaxCapacity)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            // We are running both loops until either of them is stopped or runs dry ...
            await Task
                .WhenAny(SetupPostgresAsync(stoppingToken), ProcessChannelAsync(stoppingToken))
                .ConfigureAwait(false);

            // Initializes the Postgres Listener by issueing a LISTEN Command.
            async Task SetupPostgresAsync(CancellationToken cancellationToken)
            {
                // Open a new Connection, which can be used to issue the LISTEN Command 
                // to the Postgres Database.
                using var connection = await _npgsqlDataSource
                    .OpenConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);

                // If we receive a message from Postgres, we convert the Event 
                // to a PostgresNotification and put it on the Channel.
                connection.Notification += (sender, x) =>
                {
                    var notification = new PostgresNotification
                    {
                        Channel = x.Channel,
                        PID = x.PID,
                        Payload = x.Payload,
                    };

                    channel.Writer.TryWrite(notification);
                };

                // We register to the Notifications on the Channel.
                using (var command = new NpgsqlCommand($"LISTEN {_options.ChannelName}", connection))
                {
                    await command
                        .ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                // And now we are putting the Connection into the Wait State,
                // until the Cancellation is requested.
                while (!cancellationToken.IsCancellationRequested)
                {
                    await connection
                        .WaitAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            // This Processes the Messages received by the Channel, so we can process 
            // the messages. All we are doing is basically ivoking the handler given 
            // to us.
            async Task ProcessChannelAsync(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                        {
                            await _postgresNotificationHandler.HandleNotificationAsync(message, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An Error Occured processing the Event");
                    }
                }
            }
        }
    }
}