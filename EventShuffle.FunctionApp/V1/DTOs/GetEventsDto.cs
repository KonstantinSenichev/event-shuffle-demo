using EventShuffle.Persistence.Models;
using System.Collections.Generic;
using System.Linq;

namespace EventShuffle.FunctionApp.V1.DTOs
{
    public class GetEventsDto
    {
        public ICollection<EventListItemOutputDto> Events { get; set; }

        public class EventListItemOutputDto
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public static GetEventsDto From(IEnumerable<EventModel> eventModels)
        {
            var result = new GetEventsDto()
            {
                Events = eventModels.Select(x => new EventListItemOutputDto()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList()
            };
            return result;
        }
    }
}
