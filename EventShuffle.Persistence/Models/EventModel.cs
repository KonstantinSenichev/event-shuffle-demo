using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventShuffle.Persistence.Models
{
    public class EventModel
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<EventDateModel> Dates { get; set; }
        public ICollection<VoteModel> Votes { get; set; }
    }
}
