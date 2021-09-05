using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventShuffle.FunctionApp.V1;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.FunctionApp.V1.Handlers;
using EventShuffle.Persistence;
using EventShuffle.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventShuffle.Tests.V1
{
    public class GetEventHandlerTest
    {
        [Fact]
        public async Task NoEventsExist_Get_ReturnsNotFound()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventHandler(new EventShuffleRepository(context));

            // Act
            var actual = await target.GetEventAsync(123);

            // Assert
            var result = actual as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SingleEventExists_GetWrongId_ReturnsNotFound()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventHandler(new EventShuffleRepository(context));

            var existingEvent = new EventModel()
            {
                Name = "TestEvent1",
                Dates = new List<EventDateModel>() { new EventDateModel() { Date = new DateTime(2021, 9, 20) }, new EventDateModel() { Date = new DateTime(2021, 9, 21) } }
            };

            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;
            await context.SaveChangesAsync();

            // Act
            var wrongId = existingEvent.Id + 123;
            var actual = await target.GetEventAsync(wrongId);

            // Assert
            var result = actual as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EventWithoutVotes_Get_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventHandler(new EventShuffleRepository(context));

            var existingEvents = new List<EventModel>();
            for (var i = 1; i <= 10; i++)
            {
                var e = new EventModel()
                {
                    Name = "TestEvent" + i,
                    Dates = new List<EventDateModel>() { new EventDateModel() { Date = new DateTime(2021, 9, i) }, new EventDateModel() { Date = new DateTime(2021, 9, i + 1) } }
                };

                e = (await context.Events.AddAsync(e)).Entity;
                existingEvents.Add(e);
            }

            await context.SaveChangesAsync();

            // Act
            var existingEvent = existingEvents[1];
            var actual = await target.GetEventAsync(existingEvent.Id);

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var eDto = okObjectResult.Value as GetEventDto;
            Assert.NotNull(eDto);
            Assert.Equal(existingEvent.Id, eDto.Id);
            Assert.Equal(existingEvent.Name, eDto.Name);
            Assert.Equal(0, eDto.Votes.Count);

            Assert.Equal(existingEvent.Dates.Count, eDto.Dates.Count);
            Assert.Equal(eDto.Dates.Count, eDto.Dates.Distinct().Count());

            foreach (var date in eDto.Dates)
            {
                Assert.Contains(existingEvent.Dates, x => JsonDateTimeConverter.ToDateOnlyString(x.Date) == date);
            }
        }

        [Fact]
        public async Task EventWithVotes_Get_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventHandler(new EventShuffleRepository(context));

            var date1 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 1) })).Entity;
            var date2 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 2) })).Entity;
            var date3 = (await context.EventDates.AddAsync(new EventDateModel() { Date = new DateTime(2021, 9, 3) })).Entity;

            var user1 = (await context.Users.AddAsync(new UserModel() { Name = "User1" })).Entity;
            var user2 = (await context.Users.AddAsync(new UserModel() { Name = "User2" })).Entity;

            var e = new EventModel()
            {
                Name = "TestEvent",
                Dates = new List<EventDateModel>() { date1, date2, date3 }
            };
            e = (await context.Events.AddAsync(e)).Entity;

            var notRelevantEvent = new EventModel()
            {
                Name = "TestEvent2",
                Dates = new List<EventDateModel>() { new EventDateModel() { Date = new DateTime(2021, 8, 1) } }
            };
            await context.Events.AddAsync(notRelevantEvent);

            await context.SaveChangesAsync();

            var voteUser1Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = e.Id, EventDateId = date1.Id, UserId = user1.Id })).Entity;
            var voteUser1Day2 = (await context.Votes.AddAsync(new VoteModel() { EventId = e.Id, EventDateId = date2.Id, UserId = user1.Id })).Entity;
            var voteUser2Day1 = (await context.Votes.AddAsync(new VoteModel() { EventId = e.Id, EventDateId = date1.Id, UserId = user2.Id })).Entity;

            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventAsync(e.Id);

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var eDto = okObjectResult.Value as GetEventDto;
            Assert.NotNull(eDto);
            Assert.Equal(e.Id, eDto.Id);
            Assert.Equal(e.Name, eDto.Name);
            Assert.Equal(2, eDto.Votes.Count);

            Assert.Equal(e.Dates.Count, eDto.Dates.Count);
            Assert.Equal(eDto.Dates.Count, eDto.Dates.Distinct().Count());

            foreach (var date in eDto.Dates)
            {
                Assert.Contains(e.Dates, x => JsonDateTimeConverter.ToDateOnlyString(x.Date) == date);
            }

            var votesForDate1 = eDto.Votes.FirstOrDefault(x => x.Date == JsonDateTimeConverter.ToDateOnlyString(date1.Date));
            Assert.Equal(new List<string>() { user1.Name, user2.Name }, votesForDate1.People.OrderBy(x => x));

            var votesForDate2 = eDto.Votes.FirstOrDefault(x => x.Date == JsonDateTimeConverter.ToDateOnlyString(date2.Date));
            Assert.Equal(new List<string>() { user1.Name }, votesForDate2.People.OrderBy(x => x));
        }
    }
}
