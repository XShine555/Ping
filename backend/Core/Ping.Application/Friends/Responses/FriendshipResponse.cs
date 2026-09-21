using Ping.Application.Contracts;
using Ping.Application.Users.Responses;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends.Responses
{
    public record FriendshipResponse(
        Guid Id, UserResponse OtherUser, FriendshipStatus Status, bool IsIncoming, DateTime CreatedAt, PresenceStatus Presence)
    {
        public static FriendshipResponse FromEntity(Friendship friendship, long currentUserId, PresenceStatus presence = PresenceStatus.Offline)
        {
            var isIncoming = friendship.AddresseeId == currentUserId;
            var other = isIncoming ? friendship.Requester : friendship.Addressee;

            return new FriendshipResponse(
                friendship.Id,
                UserResponse.FromEntity(other!),
                friendship.Status,
                isIncoming,
                friendship.CreatedAt,
                presence);
        }
    }
}
