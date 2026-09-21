namespace Ping.Application.Contracts
{
    public record CallAccessToken(string Jwt, string RoomName);

    public interface ICallTokenService
    {
        CallAccessToken CreateAccessToken(string roomName, string identity, string? displayName);
    }
}
