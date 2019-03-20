using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMSM.Data.Models;
namespace OpenMSM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessagesSession>().HasKey(t => new { t.MessageId, t.SessionId });
            modelBuilder.Entity<MessagesSession>().HasOne(m => m.Session).WithMany(m => m.MessagesSessions).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<ChannelsSecurityTokens>().HasKey(cst => new { cst.SecurityTokenId, cst.ChannelId });
            modelBuilder.Entity<Session>().Property(m => m.Type).HasConversion<int>();
            modelBuilder.Entity<Channel>().Property(m => m.Type).HasConversion<int>();
            modelBuilder.Entity<Message>().Property(m => m.Type).HasConversion<int>();
            //modelBuilder.Entity<ChannelsSecurityTokens>().HasOne(cst => cst.SecurityToken).WithMany(st => st.ChannelsSecurityTokens).HasForeignKey(cst => cst.SecurityTokenId);
            //modelBuilder.Entity<ChannelsSecurityTokens>().HasOne(cst => cst.Channel).WithMany(c => c.ChannelsSecurityTokens).HasForeignKey(cst => cst.ChannelId);
        }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelsSecurityTokens> ChannelsSecurityTokens { get; set; }
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagesSession> MessagesSessions { get; set; }
        public DbSet<MessageTopic> MessageTopics { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionTopic> SessionTopics { get; set; }
        public DbSet<SessionNamespace> SessionNamespaces { get; set; }
    }
}
