using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class GetEventResultsHandler
    {
        private readonly EventShuffleRepository _repository;

        public GetEventResultsHandler(EventShuffleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> GetEventResultsAsync(long id)
        {
            var eventModel = await _repository.GetEventAsync(id);
            if (eventModel is null)
            {
                return new NotFoundObjectResult($"Event with ID '{id}' not found");
            }

            var eventVotes = await _repository.GetVotesAsync(id);

            var allVotedUsers = eventVotes.Select(x => x.User.Id).Distinct().Count();

            var result = GetEventResultsDto.From(eventModel, eventVotes, allVotedUsers);
            return new OkObjectResult(result);
        }
    }
}
