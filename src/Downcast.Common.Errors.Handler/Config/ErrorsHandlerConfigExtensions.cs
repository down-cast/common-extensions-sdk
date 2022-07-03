using Downcast.Common.Errors.Handler.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Downcast.Common.Errors.Handler.Config;

public static class ErrorsHandlerConfigExtensions
{
    public static void ConfigureErrorHandlerOptions(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("errors-settings.json", true, false);
        builder.Services.AddOptions<ErrorsOptions>()
            .Bind(builder.Configuration.GetSection(ErrorsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static void ConfigureErrorHandler(this WebApplication app)
    {
        app.UseMiddleware<CustomExceptionHandler>();
    }
}