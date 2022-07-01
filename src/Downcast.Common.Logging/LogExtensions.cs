using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;

using ILogger = Serilog.ILogger;

namespace Downcast.Common.Logging;

public static class LogExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("serilog-settings.json", false, false);
        builder.Logging.ClearProviders();
        ILogger logger = Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration).CreateLogger();

        builder.Host.UseSerilog(logger);
    }
}