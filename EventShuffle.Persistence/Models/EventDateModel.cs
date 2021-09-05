using System;
using System.ComponentModel.DataAnnotations;

namespace EventShuffle.Persistence.Models
{
    public class EventDateModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        // Add more event date properties here if needed,
        // e.g. time interval etc.
    }
}
