using EventShuffle.FunctionApp.V1.Handlers;
using EventShuffle.Persistence;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(EventShuffle.FunctionApp.Startup))]

namespace EventShuffle.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            builder.Services.AddDbContext<EventShuffleDbContext>(
              options => options.UseSqlServer(connectionString));

            // BTW, do you know if Scrutor can be used already with Azure Functions without hacks?
            // I'd prefer to scan the assembly for e.g. IEventShuffleHandler implementations instead of adding bindings here manually
            builder.Services.AddScoped<CreateEventHandler>();
            builder.Services.AddScoped<CreateVoteHandler>();
            builder.Services.AddScoped<GetEventHandler>();
            builder.Services.AddScoped<GetEventsHandler>();
            builder.Services.AddScoped<GetEventResultsHandler>();

            builder.Services.AddScoped<EventShuffleRepository>();
        }
    }

#if DEBUG
    // For some reason EF CLI tools do not work properly with Azure Functions yet
    // This code is needed for EF tools in order to generate migrations and update database
    public class EventShuffleDbContextFactory : IDesignTimeDbContextFactory<EventShuffleDbContext>
    {
        public EventShuffleDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventShuffleDbContext>();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["Values:DbConnectionString"];
            optionsBuilder.UseSqlServer(connectionString);

            return new EventShuffleDbContext(optionsBuilder.Options);
        }
    }
#endif
}
