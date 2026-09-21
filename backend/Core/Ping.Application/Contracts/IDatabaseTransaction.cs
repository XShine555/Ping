namespace Ping.Application.Contracts
{
    public interface IDatabaseTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);
    }
}
