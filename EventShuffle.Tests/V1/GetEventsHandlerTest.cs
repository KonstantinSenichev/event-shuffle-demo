using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventShuffle.FunctionApp.V1.DTOs;
using EventShuffle.FunctionApp.V1.Handlers;
using EventShuffle.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventShuffle.Tests.V1
{
    public class GetEventsHandlerTest
    {
        [Fact]
        public async Task NoEventsExist_Get_ReturnsEmpty()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventsHandler(context);

            // Act
            var actual = await target.GetEventsAsync();

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var events = okObjectResult.Value as GetEventsDto;
            Assert.NotNull(events);
            Assert.Empty(events.Events);
        }

        [Fact]
        public async Task SingleEventExists_Get_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventsHandler(context);

            var existingEvent = new EventModel()
            {
                Name = "TestEvent1",
                Dates = new List<EventDateModel>() { new EventDateModel() { Date = new DateTime(2021, 9, 20) }, new EventDateModel() { Date = new DateTime(2021, 9, 21) } }
            };

            existingEvent = (await context.Events.AddAsync(existingEvent)).Entity;
            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventsAsync();

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var events = okObjectResult.Value as GetEventsDto;
            Assert.NotNull(events);
            Assert.Single(events.Events);
            Assert.Equal(existingEvent.Id, events.Events.First().Id);
            Assert.Equal(existingEvent.Name, events.Events.First().Name);
        }

        [Fact]
        public async Task SeveralEventsExist_Get_ReturnsOk()
        {
            // Arrange
            await using var context = InMemoryDbFactory.CreateDbContext();
            var target = new GetEventsHandler(context);

            var existingEvents = new List<EventModel>();
            for (var i = 1; i <= 10; i++)
            {
                var e = new EventModel()
                {
                    Name = "TestEvent" + i
                };

                e = (await context.Events.AddAsync(e)).Entity;
                existingEvents.Add(e);
            }

            await context.SaveChangesAsync();

            // Act
            var actual = await target.GetEventsAsync();

            // Assert
            var okObjectResult = actual as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var events = okObjectResult.Value as GetEventsDto;
            Assert.NotNull(events);
            Assert.Equal(existingEvents.Count, events.Events.Count);

            foreach (var eventListDto in events.Events)
            {
                var existingEvent = existingEvents.FirstOrDefault(x => x.Id == eventListDto.Id);
                Assert.NotNull(existingEvent);
                Assert.Equal(existingEvent.Name, eventListDto.Name);
            }
        }
    }
}
