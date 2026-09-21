namespace Ping.Application.Contracts
{
    public enum PresenceStatus
    {
        Offline,
        Online,
        Busy
    }

    public interface IPresenceTracker
    {
        PresenceStatus GetStatus(long userId);
    }
}
