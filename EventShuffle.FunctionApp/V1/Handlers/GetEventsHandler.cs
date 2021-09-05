using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class GetEventsHandler
    {
        private readonly EventShuffleDbContext _dbContext;

        public GetEventsHandler(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> GetEventsAsync()
        {
            var events = await _dbContext.Events.ToListAsync();
            var result = GetEventsDto.From(events);
            return new OkObjectResult(result);
        }
    }
}
