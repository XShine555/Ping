using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Shared;
using Ping.Domain.ValueObjects;

namespace Ping.Api.Hubs;

[Authorize]
public sealed class ChatHub(IDatabase database, PresenceTracker presence) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (presence.AddConnection(GetUserId()))
            await BroadcastPresenceAsync(GetUserId());

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (presence.RemoveConnection(GetUserId()))
            await BroadcastPresenceAsync(GetUserId());

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SetBusy(bool busy)
    {
        var userId = GetUserId();
        presence.SetBusy(userId, busy);
        await BroadcastPresenceAsync(userId);
    }

    private async Task BroadcastPresenceAsync(long userId)
    {
        var friendIds = await database.Friendships.AsNoTracking()
            .Where(f => f.Status == FriendshipStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(CancellationToken.None);

        if (friendIds.Count == 0)
            return;

        await Clients.Users(friendIds.Select(id => id.ToString()))
            .SendAsync("PresenceChanged", userId.ToString(), presence.GetStatus(userId).ToString());
    }

    public async Task JoinChannel(Guid channelId)
    {
        var hasAccess = await database.HasAccessAsync(channelId, GetUserId(), Context.ConnectionAborted);
        if (!hasAccess)
            throw new HubException("Channel not found");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(channelId));
    }

    public Task LeaveChannel(Guid channelId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(channelId));

    public Task Typing(Guid channelId) =>
        Clients.OthersInGroup(GroupName(channelId)).SendAsync("Typing", GetUserId(), channelId);

    public async Task RingUser(Guid channelId, string calleeId)
    {
        var callerId = GetUserId();
        if (!long.TryParse(calleeId, out var calleeUserId))
            throw new HubException("Invalid user id");

        var callerHasAccess = await database.HasAccessAsync(channelId, callerId, Context.ConnectionAborted);
        var calleeHasAccess = await database.HasAccessAsync(channelId, calleeUserId, Context.ConnectionAborted);
        if (!callerHasAccess || !calleeHasAccess)
            throw new HubException("Channel not found");

        await Clients.User(calleeId).SendAsync("IncomingCall", channelId, callerId.ToString());
    }

    public Task DeclineCall(Guid channelId, string callerId) =>
        Clients.User(callerId).SendAsync("CallDeclined", channelId);

    public Task CancelCall(Guid channelId, string calleeId) =>
        Clients.User(calleeId).SendAsync("CallCancelled", channelId);

    private long GetUserId()
    {
        var rawId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(rawId, out var id) ? id : throw new HubException("User Id claim is missing or invalid");
    }

    internal static string GroupName(Guid channelId) => $"channel:{channelId}";
}
