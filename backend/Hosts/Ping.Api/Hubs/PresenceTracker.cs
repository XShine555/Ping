using System.Collections.Concurrent;
using Ping.Application.Contracts;

namespace Ping.Api.Hubs;

public sealed class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<long, int> _connectionCounts = new();
    private readonly ConcurrentDictionary<long, byte> _busyUserIds = new();

    public bool AddConnection(long userId) => _connectionCounts.AddOrUpdate(userId, 1, (_, count) => count + 1) == 1;

    public bool RemoveConnection(long userId)
    {
        while (_connectionCounts.TryGetValue(userId, out var count))
        {
            if (count <= 1)
            {
                if (!_connectionCounts.TryRemove(userId, out _))
                    continue;

                _busyUserIds.TryRemove(userId, out _);
                return true;
            }

            if (_connectionCounts.TryUpdate(userId, count - 1, count))
                return false;
        }

        return false;
    }

    public void SetBusy(long userId, bool busy)
    {
        if (busy)
            _busyUserIds[userId] = 0;
        else
            _busyUserIds.TryRemove(userId, out _);
    }

    public PresenceStatus GetStatus(long userId)
    {
        if (!_connectionCounts.ContainsKey(userId))
            return PresenceStatus.Offline;

        return _busyUserIds.ContainsKey(userId) ? PresenceStatus.Busy : PresenceStatus.Online;
    }
}
