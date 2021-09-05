using System;

namespace EventShuffle.Persistence.Models
{
    public class VoteModel
    {
        public long EventId { get; set; }

        public long EventDateId { get; set; }
        public EventDateModel EventDate { get; set; }

        public long UserId { get; set; }
        public UserModel User { get; set; }
    }
}
