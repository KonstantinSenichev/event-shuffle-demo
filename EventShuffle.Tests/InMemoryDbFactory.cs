using EventShuffle.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace EventShuffle.Tests
{
    public static class InMemoryDbFactory
    {
        public static EventShuffleDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<EventShuffleDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // Ignore warning about not supporting transactions
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new EventShuffleDbContext(options);
        }
    }
}
