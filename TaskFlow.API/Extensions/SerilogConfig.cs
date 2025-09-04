using Serilog;

namespace TaskFlow.API.Extensions
{
    public static class SerilogConfig
    {
        public static IHostBuilder EnableSerilog(this IHostBuilder hostBuilder)
        {
            // Configure Serilog with settings from appsettings.json
            hostBuilder.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            });

            return hostBuilder;
        }
    }
}
