using Downcast.Common.Errors.Handler.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Downcast.Common.Errors.Handler.Config;

public static class ErrorsHandlerConfigExtensions
{
    /// <summary>
    /// Adds a json file to the Configuration object and configures a <see cref="ErrorsOptions"/> instance
    /// that contains details about each <see cref="ErrorCodes"/>
    /// </summary>
    /// <param name="builder"></param>
    public static void AddErrorHandlerOptions(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("errors-settings.json", true, false);
        builder.Services.AddOptions<ErrorsOptions>()
            .Bind(builder.Configuration.GetSection(ErrorsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    /// <summary>
    /// Adds <see cref="CustomExceptionHandler"/> middleware to the pipeline.
    /// This middleware will catch and handle all exceptions and return a custom error response to the client,
    /// so it should be placed at the start of the pipeline.
    /// </summary>
    /// <param name="app"></param>
    public static void UseErrorHandler(this WebApplication app)
    {
        app.UseMiddleware<CustomExceptionHandler>();
    }
}