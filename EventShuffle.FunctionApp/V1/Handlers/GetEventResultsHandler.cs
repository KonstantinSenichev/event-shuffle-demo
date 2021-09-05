using System.Linq;
using System.Threading.Tasks;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class GetEventResultsHandler
    {
        private readonly EventShuffleDbContext _dbContext;

        public GetEventResultsHandler(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> GetEventResultsAsync(long id)
        {
            // This could be moved to UnitOfWork/Repository as soon as we have lot's of similar logic in many places
            var eventModel = await _dbContext.Events
                .Include(x => x.Dates)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (eventModel is null)
            {
                return new NotFoundObjectResult($"Event with ID '{id}' not found");
            }

            var eventVotes = _dbContext.Votes
                .Include(x => x.EventDate)
                .Include(x => x.User)
                .Where(x => x.EventId == id)
                .OrderBy(x => x.EventDate.Date);

            var allVotedUsers = await eventVotes.Select(x => x.User.Id).Distinct().CountAsync();

            var result = GetEventResultsDto.From(eventModel, await eventVotes.ToListAsync(), allVotedUsers);
            return new OkObjectResult(result);
        }
    }
}
