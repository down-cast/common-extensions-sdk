using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;

using ILogger = Serilog.ILogger;

namespace Downcast.Common.Logging;

public static class LogExtensions
{
    /// <summary>
    /// Adds a new file to the Configuration object and configures the Serilog logger.
    /// </summary>
    /// <param name="builder"></param>
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("serilog-settings.json", false, false);
        builder.Logging.ClearProviders();
        ILogger logger = Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog(logger);
    }
}