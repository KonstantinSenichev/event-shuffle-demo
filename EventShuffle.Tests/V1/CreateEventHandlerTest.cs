using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.FunctionApp.V1.Handlers;
using EventShuffle.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventShuffle.Tests.V1
{
    public class CreateEventHandlerTest
    {
        [Fact]
        public async Task EmptyName_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateEventHandler(context);

            // Act
            var e = new CreateEventInputDto() { Name = string.Empty, Dates = new List<DateTime>() { DateTime.Now } };
            var actual = await target.CreateEventAsync(e);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EmptyDates_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateEventHandler(context);

            // Act
            var e = new CreateEventInputDto() { Name = "TestEvent", Dates = new List<DateTime>() };
            var actual = await target.CreateEventAsync(e);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DuplicatedDates_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateEventHandler(context);

            var date1 = DateTime.Now;
            var date2 = date1.AddDays(1);

            // Act
            var e = new CreateEventInputDto() { Name = "TestEvent", Dates = new List<DateTime>() { date1, date2, date1 } };
            var actual = await target.CreateEventAsync(e);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EventExists_CreateSameEvent_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateEventHandler(context);

            var existingEvent = new EventModel()
            {
                Name = "TestEvent1",
                Dates = new List<EventDateModel>() { new EventDateModel() { Date = new DateTime(2021, 9, 20) }, new EventDateModel() { Date = new DateTime(2021, 9, 21) } }
            };

            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;
            await context.SaveChangesAsync();

            // Act
            var e = new CreateEventInputDto() { Name = existingEvent.Name, Dates = new List<DateTime>() { DateTime.Now } };
            var actual = await target.CreateEventAsync(e);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EventDoNotExist_Create_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateEventHandler(context);

            // Act
            var e = new CreateEventInputDto() { Name = "TestEvent", Dates = new List<DateTime>() { new DateTime(2021, 9, 20), new DateTime(2021, 9, 21) } };
            var actual = await target.CreateEventAsync(e);

            // Assert
            var result = actual as OkObjectResult;
            Assert.NotNull(result);

            var cDto = result.Value as CreateEventOutputDto;
            Assert.NotNull(cDto);

            var id = cDto.Id;
            var createdEvent = await context.Events.Include(x => x.Dates).FirstOrDefaultAsync(x => x.Id == id);
            Assert.NotNull(createdEvent);
            Assert.Equal(e.Name, createdEvent.Name);
            Assert.Equal(e.Dates.Count, createdEvent.Dates.Count);
        }
    }
}
