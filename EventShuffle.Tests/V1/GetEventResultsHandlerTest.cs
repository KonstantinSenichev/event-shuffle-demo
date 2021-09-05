using EventShuffle.FunctionApp.V1;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.FunctionApp.V1.Handlers;
using EventShuffle.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EventShuffle.Tests.V1
{
    public class GetEventResultsHandlerTest
    {
        [Fact]
        public async Task EventNotExists_GetResults_ReturnsNotFound()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventResultsHandler(context);

            // Act
            var actual = await target.GetEventResultsAsync(123);

            // Assert
            var result = actual as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EmptyVotes_GetResults_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventResultsHandler(context);

            var date1 = new DateTime(2021, 9, 20);
            var date2 = new DateTime(2021, 9, 21);

            var existingEvent = new EventModel()
            {
                Name = "TestEvent1",
                Dates = new List<EventDateModel>() { new EventDateModel() { Date = date1 }, new EventDateModel() { Date = date2 } }
            };

            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;
            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventResultsAsync(existingEvent.Id);

            // Assert
            var result = actual as OkObjectResult;
            Assert.NotNull(result);

            var dto = result.Value as GetEventResultsDto;
            Assert.NotNull(dto);
            Assert.Equal(existingEvent.Id, dto.Id);
            Assert.Equal(existingEvent.Name, dto.Name);
            Assert.Empty(dto.SuitableDates);
        }

        [Fact]
        public async Task DayOneSuits_GetResults_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventResultsHandler(context);

            var date1 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 1) })).Entity;
            var date2 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 2) })).Entity;
            var date3 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 3) })).Entity;

            var user1 = (await context.Users.AddAsync(new UserModel() { Name = "User1" })).Entity;
            var user2 = (await context.Users.AddAsync(new UserModel() { Name = "User2" })).Entity;

            var existingEvent = new EventModel()
            {
                Name = "TestEvent",
                Dates = new List<EventDateModel>() { date1, date2, date3 }
            };
            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;

            await context.SaveChangesAsync();

            var voteUser1Day1 = (await context.Votes.AddAsync(new VoteModel()
                { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user1.Id })).Entity;
            var voteUser1Day2 = (await context.Votes.AddAsync(new VoteModel()
                { EventId = existingEvent.Id, EventDateId = date2.Id, UserId = user1.Id })).Entity;
            var voteUser2Day1 = (await context.Votes.AddAsync(new VoteModel()
                { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventResultsAsync(existingEvent.Id);

            // Assert
            var result = actual as OkObjectResult;
            Assert.NotNull(result);

            var dto = result.Value as GetEventResultsDto;
            Assert.NotNull(dto);
            Assert.Equal(existingEvent.Id, dto.Id);
            Assert.Equal(existingEvent.Name, dto.Name);
            Assert.Single(dto.SuitableDates);
            Assert.Equal(JsonDateTimeConverter.ToDateOnlyString(date1.Date), dto.SuitableDates.First().Date);
            Assert.Equal(new List<string>() { user1.Name, user2.Name },
                dto.SuitableDates.First().People.OrderBy(x => x));
        }

        [Fact]
        public async Task DayTwoSuits_GetResults_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventResultsHandler(context);

            var date1 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 1) })).Entity;
            var date2 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 2) })).Entity;
            var date3 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 3) })).Entity;

            var user1 = (await context.Users.AddAsync(new UserModel() { Name = "User1" })).Entity;
            var user2 = (await context.Users.AddAsync(new UserModel() { Name = "User2" })).Entity;

            var existingEvent = new EventModel()
            {
                Name = "TestEvent",
                Dates = new List<EventDateModel>() { date1, date2, date3 }
            };
            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;

            await context.SaveChangesAsync();

            var voteUser1Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user1.Id })).Entity;
            var voteUser1Day2 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date2.Id, UserId = user1.Id })).Entity;
            var voteUser2Day2 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date2.Id, UserId = user2.Id })).Entity;
            var voteUser2Day3 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date3.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventResultsAsync(existingEvent.Id);

            // Assert
            var result = actual as OkObjectResult;
            Assert.NotNull(result);

            var dto = result.Value as GetEventResultsDto;
            Assert.NotNull(dto);
            Assert.Equal(existingEvent.Id, dto.Id);
            Assert.Equal(existingEvent.Name, dto.Name);
            Assert.Single(dto.SuitableDates);
            Assert.Equal(JsonDateTimeConverter.ToDateOnlyString(date2.Date), dto.SuitableDates.First().Date);
            Assert.Equal(new List<string>() { user1.Name, user2.Name }, dto.SuitableDates.First().People.OrderBy(x => x));
        }

        [Fact]
        public async Task NoDaysSuit_GetResults_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventResultsHandler(context);

            var date1 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 1) })).Entity;
            var date2 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 2) })).Entity;
            var date3 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 3) })).Entity;

            var user1 = (await context.Users.AddAsync(new UserModel() { Name = "User1" })).Entity;
            var user2 = (await context.Users.AddAsync(new UserModel() { Name = "User2" })).Entity;

            var existingEvent = new EventModel()
            {
                Name = "TestEvent",
                Dates = new List<EventDateModel>() { date1, date2, date3 }
            };
            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;

            await context.SaveChangesAsync();

            var voteUser1Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user1.Id })).Entity;
            var voteUser2Day3 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date3.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventResultsAsync(existingEvent.Id);

            // Assert
            var result = actual as OkObjectResult;
            Assert.NotNull(result);

            var dto = result.Value as GetEventResultsDto;
            Assert.NotNull(dto);
            Assert.Equal(existingEvent.Id, dto.Id);
            Assert.Equal(existingEvent.Name, dto.Name);
            Assert.Empty(dto.SuitableDates);
        }
    }
}
