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
    public class CreateVoteHandlerTest
    {
        [Fact]
        public async Task EmptyName_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var vote = new CreateVoteInputDto() { Name = string.Empty, Votes = new List<DateTime>() { date1 } };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EmptyDates_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var vote = new CreateVoteInputDto() { Name = "Nick", Votes = new List<DateTime>() };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DuplicatedDates_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var vote = new CreateVoteInputDto() { Name = "Nick", Votes = new List<DateTime>() {date1, date2, date1} };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EventNotExists_Create_ReturnsNotFound()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

            var date1 = new DateTime(2021, 9, 20);
            var date2 = new DateTime(2021, 9, 21);

            // Act
            var vote = new CreateVoteInputDto() { Name = "Nick", Votes = new List<DateTime>() { date1, date2 } };
            var actual = await target.CreateVoteAsync(123, vote);

            // Assert
            var result = actual as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task WrongDate_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var vote = new CreateVoteInputDto() { Name = "Nick", Votes = new List<DateTime>() { date2.AddDays(10) } };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task VoteForDateAlreadyExists_Create_ReturnsBadRequest()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var voteUser2Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var vote = new CreateVoteInputDto() { Name = user2.Name, Votes = new List<DateTime>() { date1.Date } };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var result = actual as BadRequestObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CorrectVote_Create_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new CreateVoteHandler(context);

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
            var voteUser2Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = existingEvent.Id, EventDateId = date1.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var vote = new CreateVoteInputDto() { Name = user2.Name, Votes = new List<DateTime>() { date3.Date } };
            var actual = await target.CreateVoteAsync(existingEvent.Id, vote);

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var eDto = okObjectResult.Value as GetEventDto;
            Assert.NotNull(eDto);
            Assert.Equal(existingEvent.Id, eDto.Id);
            Assert.Equal(existingEvent.Name, eDto.Name);
            Assert.Equal(3, eDto.Votes.Count);

            Assert.Equal(existingEvent.Dates.Count, eDto.Dates.Count);
            Assert.Equal(eDto.Dates.Count, eDto.Dates.Distinct().Count());

            foreach (var date in eDto.Dates)
            {
                Assert.Contains(existingEvent.Dates, x => JsonDateTimeConverter.ToDateOnlyString(x.Date) == date);
            }

            var votesForDate1 = eDto.Votes.FirstOrDefault(x => x.Date == JsonDateTimeConverter.ToDateOnlyString(date1.Date));
            Assert.Equal(new List<string>() { user1.Name, user2.Name }, votesForDate1.People.OrderBy(x => x));

            var votesForDate2 = eDto.Votes.FirstOrDefault(x => x.Date == JsonDateTimeConverter.ToDateOnlyString(date2.Date));
            Assert.Equal(new List<string>() { user1.Name }, votesForDate2.People.OrderBy(x => x));

            var votesForDate3 = eDto.Votes.FirstOrDefault(x => x.Date == JsonDateTimeConverter.ToDateOnlyString(date3.Date));
            Assert.Equal(new List<string>() { user2.Name }, votesForDate3.People.OrderBy(x => x));
        }
    }
}
