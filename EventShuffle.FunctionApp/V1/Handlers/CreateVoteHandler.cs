using System;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.Persistence;
using EventShuffle.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace EventShuffle.FunctionApp.V1.Handlers
{
    public class CreateVoteHandler
    {
        private readonly EventShuffleDbContext _dbContext;

        public CreateVoteHandler(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> CreateVoteAsync(long eventId, CreateVoteInputDto inputDto)
        {
            // This could be moved to UnitOfWork/Repository as soon as we have lot's of similar logic in many places
            var eventModel = await _dbContext.Events
                .Include(x => x.Dates)
                .FirstOrDefaultAsync(x => x.Id == eventId);
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

            // Make sure user exists in DB, add if needed
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name.ToLower() == inputDto.Name);
            if (user is null)
            {
                user = new UserModel() { Name = inputDto.Name };
                await _dbContext.Users.AddAsync(user);
            }

            var dateModels = new List<EventDateModel>();
            foreach (var date in inputDto.Votes)
            {
                var dateModel = eventModel.Dates.FirstOrDefault(x => x.Date == date);
                if (dateModel is null)
                {
                    return new BadRequestObjectResult($"Event '{eventModel.Name}' does not have date '{JsonDateTimeConverter.ToDateOnlyString(date)}' suggested");
                }
                dateModels.Add(dateModel);

                // ToLowerInvariant() is not supported by SQL, so ToLower() is the way to go here
                var existingVote = await _dbContext.Votes.FirstOrDefaultAsync(x =>
                    x.EventId == eventId && x.EventDate.Id == dateModel.Id &&
                    x.User.Name.ToLower() == inputDto.Name.ToLower());

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

            var eventVotes = await _dbContext.Votes
                .Include(x => x.EventDate)
                .Include(x => x.User)
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.EventDate.Date)
                .ToListAsync();

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
