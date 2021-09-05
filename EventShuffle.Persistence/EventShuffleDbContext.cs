using EventShuffle.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventShuffle.Persistence
{
    public class EventShuffleDbContext : DbContext
    {
        public EventShuffleDbContext(DbContextOptions<EventShuffleDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventModel>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<UserModel>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<VoteModel>().HasKey(x => new { x.EventId, x.EventDateId, x.UserId });

            modelBuilder.Entity<EventDateModel>()
                .Property(e => e.Date)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }

        public DbSet<EventModel> Events { get; set; }
        public DbSet<EventDateModel> EventDates { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<VoteModel> Votes { get; set; }
    }
}
