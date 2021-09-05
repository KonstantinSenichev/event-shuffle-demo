using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class GetEventHandler
    {
        private readonly EventShuffleRepository _repository;

        public GetEventHandler(EventShuffleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> GetEventAsync(long id)
        {
            var eventModel = await _repository.GetEventAsync(id);
            if (eventModel is null)
            {
                return new NotFoundObjectResult($"Event with ID '{id}' not found");
            }

            var eventVotes = await _repository.GetVotesAsync(id);

            var result = GetEventDto.From(eventModel, eventVotes);
            return new OkObjectResult(result);
        }
    }
}
