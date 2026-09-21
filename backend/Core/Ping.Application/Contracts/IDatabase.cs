using Microsoft.EntityFrameworkCore;
using Ping.Domain.Entities;

namespace Ping.Application.Contracts
{
    public interface IDatabase
    {
        DbSet<User> Users { get; }

        DbSet<Friendship> Friendships { get; }

        DbSet<Server> Servers { get; }

        DbSet<ServerMember> ServerMembers { get; }

        DbSet<Channel> Channels { get; }

        DbSet<ChannelMember> ChannelMembers { get; }

        DbSet<Message> Messages { get; }

        DbSet<Attachment> Attachments { get; }

        Task<IDatabaseTransaction> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
