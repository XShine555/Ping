using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;

namespace Ping.Application.Shared
{
    public static class ChannelAccess
    {
        public static async Task<bool> HasAccessAsync(this IDatabase database, Guid channelId, long userId, CancellationToken cancellationToken)
        {
            var isDirectMessageParticipant = await database.ChannelMembers
                .AnyAsync(m => m.ChannelId == channelId && m.UserId == userId, cancellationToken);

            if (isDirectMessageParticipant)
                return true;

            return await database.Channels.AnyAsync(c =>
                c.Id == channelId && c.ServerId != null &&
                database.ServerMembers.Any(sm => sm.ServerId == c.ServerId && sm.UserId == userId),
                cancellationToken);
        }
    }
}
