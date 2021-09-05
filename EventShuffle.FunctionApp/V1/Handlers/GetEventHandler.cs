using System.Linq;
using System.Threading.Tasks;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class GetEventHandler
    {
        private readonly EventShuffleDbContext _dbContext;

        public GetEventHandler(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> GetEventAsync(long id)
        {
            var eventModel = await _dbContext.Events
                .Include(x => x.Dates)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (eventModel is null)
            {
                return new NotFoundObjectResult($"Event with ID '{id}' not found");
            }

            var eventVotes = await _dbContext.Votes
                .Include(x => x.EventDate)
                .Include(x => x.User)
                .Where(x => x.EventId == id)
                .OrderBy(x => x.EventDate.Date)
                .ToListAsync();

            var result = GetEventDto.From(eventModel, eventVotes);
            return new OkObjectResult(result);
        }
    }
}
