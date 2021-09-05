# EventShuffle Demo

This repository contains cloud-native implementation of the EventShuffle REST API.

## Utilized technologies:
- C#, .NET Core 3.1
- Azure Functions v3
- Entity Framework Core
- FluentValidation 
- xUnit.net

## Solution structure
- `EventShuffle.FunctionApp` contains REST API implementation on top of HTTP-triggered Azure Functions.
- `EventShuffle.Persistence` implements data storage functionality using EF Core for SQL server.
- `EventShuffle.Tests` contains integration tests covering application business logic.

## Notes
Originally I intended to utilize Swagger to provide nice REST API documentation, but Azure Functions support is still in preview state there. 
So as soon as some weird library loading issues appeared - Swagger was removed from solution :|

# Building and running solution locally

## System requirements

- .NET Core 3.1 SDK, see https://dotnet.microsoft.com/download/dotnet/3.1
- Entity Framework Core Tools for the .NET Command-Line Interface
  ```
  dotnet tool install --global dotnet-ef --version 3.1.18
  ```

- Azure Functions Core tools v3 (not needed if you have Visual Studio 2019 with Azure development workload installed), follow installation instructions at https://www.npmjs.com/package/azure-functions-core-tools
- SQL Server, e.g. MS SQL Server Express, see https://www.microsoft.com/en-us/Download/details.aspx?id=101064 or https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker

## Running application

1. If needed (probably not) update `DbConnectionString` in `event-shuffle-demo/EventShuffle.FunctionApp/local.settings.json`.
2. Deploy database
  ```
  cd event-shuffle-demo
  dotnet ef database update -s EventShuffle.FunctionApp -p EventShuffle.Persistence
  ```
3. Run Azure Function App from console
  ```
  cd event-shuffle-demo/EventShuffle.FunctionApp
  func start
  ```
  Alternatively just start `EventShuffle.FunctionApp` project in Visual Studio.
4. Application console will display supported REST API URIs, e.g.
  ```
  [POST] http://localhost:7071/api/v1/event
  [POST] http://localhost:7071/api/v1/event/{id:long}/vote
  [GET] http://localhost:7071/api/v1/event/{id:long}  
  [GET] http://localhost:7071/api/v1/event/{id:long}/results  
  [GET] http://localhost:7071/api/v1/event/list
  ```
5. Access REST API in browser/Postman/etc.
6. Have fun!

  