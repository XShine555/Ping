using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ping.Domain.Entities;
using Ping.Infrastructure.Configuration;
using AppIDatabase = Ping.Application.Contracts.IDatabase;

namespace Ping.Infrastructure.Persistence
{
    public class Database(DatabaseConfiguration configuration) : DbContext, AppIDatabase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(configuration.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerMember>()
                .HasKey(m => new { m.ServerId, m.UserId });

            modelBuilder.Entity<ServerMember>()
                .HasOne(m => m.Server)
                .WithMany(s => s.Members)
                .HasForeignKey(m => m.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerMember>()
                .HasOne(m => m.User)
                .WithMany(u => u.ServerMemberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Server>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Channel>()
                .HasOne(c => c.ServerNavigation)
                .WithMany(s => s.Channels)
                .HasForeignKey(c => c.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChannelMember>()
                .HasKey(m => new { m.ChannelId, m.UserId });

            modelBuilder.Entity<ChannelMember>()
                .HasOne(m => m.Channel)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChannelMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Channel)
                .WithMany()
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Author)
                .WithMany()
                .HasForeignKey(m => m.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ChannelId, m.CreatedAt });

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Addressee)
                .WithMany()
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.AddresseeId })
                .IsUnique();
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Friendship> Friendships => Set<Friendship>();

        public DbSet<Server> Servers => Set<Server>();

        public DbSet<ServerMember> ServerMembers => Set<ServerMember>();

        public DbSet<Channel> Channels => Set<Channel>();

        public DbSet<ChannelMember> ChannelMembers => Set<ChannelMember>();

        public DbSet<Message> Messages => Set<Message>();

        public DbSet<Attachment> Attachments => Set<Attachment>();

        public async Task<Ping.Application.Contracts.IDatabaseTransaction> BeginTransactionAsync(
            System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            var transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return new DatabaseTransaction(transaction);
        }

        private sealed class DatabaseTransaction(IDbContextTransaction inner) : Ping.Application.Contracts.IDatabaseTransaction
        {
            public Task CommitAsync(CancellationToken cancellationToken) =>
                inner.CommitAsync(cancellationToken);

            public ValueTask DisposeAsync() => inner.DisposeAsync();
        }
    }
}
