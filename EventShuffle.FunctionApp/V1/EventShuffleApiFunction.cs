using EventShuffle.FunctionApp.V1.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EventShuffle.FunctionApp.V1.Handlers;

namespace EventShuffle.FunctionApp.V1
{
    public class EventShuffleApiFunction
    {
        private readonly CreateEventHandler _createEventHandler;
        private readonly CreateVoteHandler _createVoteHandler;
        private readonly GetEventHandler _getEventHandler;
        private readonly GetEventsHandler _getEventsHandler;
        private readonly GetEventResultsHandler _getEventResultsHandler;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonDateTimeConverter() }
        };

        public EventShuffleApiFunction(CreateEventHandler createEventHandler, CreateVoteHandler createVoteHandler,
            GetEventHandler getEventHandler, GetEventsHandler getEventsHandler, GetEventResultsHandler getEventResultsHandler)
        {
            // In more complex cases these injected handlers could be replaced by e.g. MediatR
            _createEventHandler = createEventHandler;
            _createVoteHandler = createVoteHandler;
            _getEventHandler = getEventHandler;
            _getEventsHandler = getEventsHandler;
            _getEventResultsHandler = getEventResultsHandler;
        }

        [FunctionName("GetEvents")]
        public async Task<IActionResult> GetEvents(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/list")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetEvents processing a request.");

            var result = await _getEventsHandler.GetEventsAsync();
            return result;
        }

        [FunctionName("GetEvent")]
        public async Task<IActionResult> GetEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{id:long}")] HttpRequest req,
            ILogger log)
        {
            var id = long.Parse((string)req.RouteValues["id"]);
            log.LogInformation($"GetEvent '{id}' processing a request.");

            var result = await _getEventHandler.GetEventAsync(id);
            return result;
        }

        [FunctionName("GetEventResults")]
        public async Task<IActionResult> GetEventResults(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{id:long}/results")] HttpRequest req,
            ILogger log)
        {
            var id = long.Parse((string)req.RouteValues["id"]);
            log.LogInformation($"GetEventResults '{id}' processing a request.");

            var result = await _getEventResultsHandler.GetEventResultsAsync(id);
            return result;
        }

        [FunctionName("CreateEvent")]
        public async Task<IActionResult> CreateEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateEvent processing a request.");

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            CreateEventInputDto eventInputDto;
            try
            {
                eventInputDto = System.Text.Json.JsonSerializer.Deserialize<CreateEventInputDto>(requestBody, _jsonSerializerOptions);
            }
            catch (Exception e)
            {
                log.LogError("CreateEvent payload parse failed: {Message}", e.Message);
                return new BadRequestObjectResult(e.Message);
            }

            var result = await _createEventHandler.CreateEventAsync(eventInputDto);
            return result;
        }

        [FunctionName("CreateVote")]
        public async Task<IActionResult> CreateVote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event/{id:long}/vote")] HttpRequest req,
            ILogger log)
        {
            var eventId = long.Parse((string)req.RouteValues["id"]);

            log.LogInformation($"CreateVote for event '{eventId}' processing a request.");

            string requestBody;
            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            CreateVoteInputDto inputDto;
            try
            {
                inputDto = System.Text.Json.JsonSerializer.Deserialize<CreateVoteInputDto>(requestBody, _jsonSerializerOptions);
            }
            catch (Exception e)
            {
                log.LogError("CreateVote payload parse failed: {Message}", e.Message);
                return new BadRequestObjectResult(e.Message);
            }

            var result = await _createVoteHandler.CreateVoteAsync(eventId, inputDto);
            return result;
        }
    }
}

