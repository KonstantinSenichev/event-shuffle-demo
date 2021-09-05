using System;
using System.Collections.Generic;

namespace EventShuffle.FunctionApp.V1.DTOs
{
    public class CreateVoteInputDto
    {
        public string Name { get; set; }

        public ICollection<DateTime> Votes { get; set; }
    }
}
