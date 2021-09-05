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
    public class CreateEventHandler
    {
        private readonly EventShuffleDbContext _dbContext;

        public CreateEventHandler(EventShuffleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> CreateEventAsync(CreateEventInputDto inputDto)
        {
            var validator = new CreateEventValidator();
            var validationResults = await validator.ValidateAsync(inputDto);
            if (!validationResults.IsValid)
            {
                var error = validationResults.Errors.First().ErrorMessage;
                return new BadRequestObjectResult(error);
            }

            // This could be moved to UnitOfWork/Repository as soon as we have lot's of similar logic in many places
            // ToLowerInvariant() is not supported by SQL, so ToLower() is the way to go here
            var sameName = await _dbContext.Events.FirstOrDefaultAsync(x => x.Name.ToLower() == inputDto.Name.ToLower());
            if (sameName != null)
            {
                return new BadRequestObjectResult($"Event named '{sameName.Name}' already exists");
            }

            var model = new EventModel()
            {
                Name = inputDto.Name,
                Dates = inputDto.Dates.Select(x => new EventDateModel() { Date = x }).ToList()
            };

            var added = await _dbContext.Events.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            var result = new CreateEventOutputDto() { Id = added.Entity.Id };
            return new OkObjectResult(result);
        }
    }

    public class CreateEventValidator : AbstractValidator<CreateEventInputDto>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Event name should not be blank");
            RuleFor(x => x.Dates).NotEmpty().WithMessage("Event should have at least one date specified");
            RuleFor(x => x.Dates).Must(HaveUniqueValues).WithMessage("Event dates should not duplicate");
        }

        private bool HaveUniqueValues(ICollection<DateTime> values)
        {
            return (values.Distinct().Count() == values.Count);
        }
    }
}
