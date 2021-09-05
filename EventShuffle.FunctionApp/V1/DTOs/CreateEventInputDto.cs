using System;
using System.Collections.Generic;

namespace EventShuffle.FunctionApp.V1.DTOs
{
    public class CreateEventInputDto
    {
        public string Name { get; set; }

        public ICollection<DateTime> Dates { get; set; }
    }
}
