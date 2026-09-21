using System.Text.Json.Serialization;
using Ping.Application.Serialization;

namespace Ping.Api.DataTransferObjects.Friends;

public record SendFriendRequestRequest([property: JsonConverter(typeof(LongAsStringConverter))] long AddresseeId);
