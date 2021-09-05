using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using EventShuffle.Persistence.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class CreateVoteHandler
    {
        private readonly EventShuffleDbContext _dbContext;
        private readonly EventShuffleRepository _repository;

        public CreateVoteHandler(EventShuffleDbContext dbContext, EventShuffleRepository repository)
        {
            _dbContext = dbContext;
            _repository = repository;
        }

        public async Task<IActionResult> CreateVoteAsync(long eventId, CreateVoteInputDto inputDto)
        {
            var eventModel = await _repository.GetEventAsync(eventId);
            if (eventModel is null)
            {
                return new NotFoundObjectResult($"Event with ID '{eventId}' not found");
            }

            var validator = new CreateVoteValidator();
            var validationResults = await validator.ValidateAsync(inputDto);
            if (!validationResults.IsValid)
            {
                var error = validationResults.Errors.First().ErrorMessage;
                return new BadRequestObjectResult(error);
            }

            var user = await _repository.UpsertUserAsync(inputDto.Name);

            var dateModels = new List<EventDateModel>();
            foreach (var date in inputDto.Votes)
            {
                var dateModel = eventModel.Dates.FirstOrDefault(x => x.Date == date);
                if (dateModel is null)
                {
                    return new BadRequestObjectResult($"Event '{eventModel.Name}' does not have date '{JsonDateTimeConverter.ToDateOnlyString(date)}' suggested");
                }
                dateModels.Add(dateModel);

                var existingVote = await _repository.GetVoteAsync(eventId, dateModel.Id, user.Name);

                if (existingVote != null)
                {
                    return new BadRequestObjectResult($"User '{inputDto.Name}' has already voted for event '{eventModel.Name}' and date '{JsonDateTimeConverter.ToDateOnlyString(date)}'");
                }
            }

            var voteModels = dateModels
                .OrderBy(x => x.Date)
                .Select(dateModel => new VoteModel()
                { EventDate = dateModel, User = user, EventId = eventId }).ToList();

            await _dbContext.Votes.AddRangeAsync(voteModels);
            await _dbContext.SaveChangesAsync();

            var eventVotes = await _repository.GetVotesAsync(eventId);

            var result = GetEventDto.From(eventModel, eventVotes);
            return new OkObjectResult(result);
        }
    }

    public class CreateVoteValidator : AbstractValidator<CreateVoteInputDto>
    {
        public CreateVoteValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("User name should not be blank");
            RuleFor(x => x.Votes).NotEmpty().WithMessage("Vote should have at least one date specified");
            RuleFor(x => x.Votes).Must(HaveUniqueValues).WithMessage("Voting for the same date twice is not allowed");
        }

        private bool HaveUniqueValues(ICollection<DateTime> values)
        {
            return (values.Distinct().Count() == values.Count);
        }
    }
}
