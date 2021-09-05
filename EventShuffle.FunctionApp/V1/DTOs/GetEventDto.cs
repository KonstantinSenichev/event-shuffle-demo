using EventShuffle.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventShuffle.FunctionApp.V1.DTOs
{
    public class GetEventDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<string> Dates { get; set; }

        public ICollection<VoteOutputDto> Votes { get; set; }

        public class VoteOutputDto
        {
            public string Date { get; set; }

            public ICollection<string> People { get; set; }
        }

        public static GetEventDto From(EventModel eventModel, ICollection<VoteModel> eventVotes)
        {
            var result = new GetEventDto
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                Dates = eventModel.Dates.OrderBy(x => x.Date).Select(x => JsonDateTimeConverter.ToDateOnlyString(x.Date)).ToList(),
                Votes = new List<VoteOutputDto>()
            };

            var eventVotesByDate = eventVotes.GroupBy(x => x.EventDate.Date);
            foreach (var votesForSameDate in eventVotesByDate)
            {
                var voteDto = new VoteOutputDto()
                {
                    Date = JsonDateTimeConverter.ToDateOnlyString(votesForSameDate.Key),
                    People = votesForSameDate.Select(x => x.User.Name).ToList()
                };
                result.Votes.Add(voteDto);
            }

            return result;
        }
    }
}
