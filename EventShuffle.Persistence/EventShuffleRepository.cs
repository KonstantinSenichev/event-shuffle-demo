using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventShuffle.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EventShuffle.Persistence
{
    public class EventShuffleRepository
    {
        private readonly EventShuffleDbContext _dbContext;

        public EventShuffleRepository(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserModel> UpsertUserAsync(string name)
        {
            // ToLowerInvariant() is not supported by SQL, so ToLower() is the way to go here
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name.ToLower() == name);
            if (user is null)
            {
                user = new UserModel() { Name = name };
                user = (await _dbContext.Users.AddAsync(user)).Entity;
            }

            return user;
        }

        public async Task<EventModel> GetEventAsync(long id)
        {
            var eventModel = await _dbContext.Events
                .Include(x => x.Dates)
                .FirstOrDefaultAsync(x => x.Id == id);
            return eventModel;
        }

        public async Task<EventModel> GetEventAsync(string name)
        {
            // ToLowerInvariant() is not supported by SQL, so ToLower() is the way to go here
            var eventModel = await _dbContext.Events
                .Include(x => x.Dates)
                .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
            return eventModel;
        }

        public async Task<VoteModel> GetVoteAsync(long eventId, long dateId, string userName)
        {
            // ToLowerInvariant() is not supported by SQL, so ToLower() is the way to go here
            var vote = await _dbContext.Votes.FirstOrDefaultAsync(x =>
                x.EventId == eventId && x.EventDate.Id == dateId &&
                x.User.Name.ToLower() == userName.ToLower());
            return vote;
        }

        public async Task<List<VoteModel>> GetVotesAsync(long eventId)
        {
            var eventVotes = await _dbContext.Votes
                .Include(x => x.EventDate)
                .Include(x => x.User)
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.EventDate.Date)
                .ToListAsync();
            return eventVotes;
        }
    }
}
