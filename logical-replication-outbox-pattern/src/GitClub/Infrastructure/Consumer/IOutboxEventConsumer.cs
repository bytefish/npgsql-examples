// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GitClub.Database.Models;

namespace GitClub.Infrastructure.Consumer
{
    public interface IOutboxEventConsumer
    {
        Task ConsumeOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken);
    }
}