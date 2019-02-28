using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ISBM.Data.Models;
namespace ISBM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessagesSession>().HasKey(t => new { t.MessageId, t.SessionId });
            modelBuilder.Entity<ChannelsSecurityTokens>().HasOne(cst => cst.SecurityToken).WithMany(st => st.ChannelsSecurityTokens).HasForeignKey(cst => cst.SecurityTokenId);
            modelBuilder.Entity<ChannelsSecurityTokens>().HasOne(cst => cst.Channel).WithMany(c => c.ChannelsSecurityTokens).HasForeignKey(cst => cst.ChannelId);
        }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelsSecurityTokens> ChannelsSecurityTokens { get; set; }
        public DbSet<SecurityToken> SecurityTokens { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagesSession> MessagesSessions { get; set; }
        public DbSet<MessageTopic> MessageTopics { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionTopic> SessionTopics { get; set; }
    }
}
