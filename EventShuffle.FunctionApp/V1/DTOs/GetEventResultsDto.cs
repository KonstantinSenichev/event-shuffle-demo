using EventShuffle.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventShuffle.FunctionApp.V1.DTOs
{
    public class GetEventResultsDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<SuitableDateDto> SuitableDates { get; set; }

        public class SuitableDateDto
        {
            public string Date { get; set; }

            public ICollection<string> People { get; set; }
        }

        public static GetEventResultsDto From(EventModel eventModel, ICollection<VoteModel> eventVotes, int votedUsers)
        {
            var result = new GetEventResultsDto
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                SuitableDates = new List<SuitableDateDto>()
            };

            var eventVotesByDate = eventVotes.GroupBy(x => x.EventDate.Date);
            foreach (var votesForSameDate in eventVotesByDate)
            {
                var suitableDateDto = new SuitableDateDto()
                {
                    Date = JsonDateTimeConverter.ToDateOnlyString(votesForSameDate.Key),
                    People = votesForSameDate.Select(x => x.User.Name).ToList()
                };
                if (suitableDateDto.People.Count == votedUsers)
                {
                    result.SuitableDates.Add(suitableDateDto);
                }
            }

            return result;
        }
    }
}
