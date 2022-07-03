using System.Net;
using System.Text.Json;

using Downcast.Common.Errors.Handler.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Downcast.Common.Errors.Handler;

public class CustomExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly IOptions<ErrorsOptions> _options;
    private readonly ILogger<CustomExceptionHandler> _logger;

    private const HttpStatusCode DefaultStatusCode = HttpStatusCode.InternalServerError;
    private const string ContentType = "application/json";

    public CustomExceptionHandler(
        RequestDelegate next,
        IOptions<ErrorsOptions> options,
        ILogger<CustomExceptionHandler> logger)
    {
        _next    = next;
        _options = options;
        _logger  = logger;
        ValidateOptions(options);
    }

    private void ValidateOptions(IOptions<ErrorsOptions> options)
    {
        if (options.Value.ErrorCodeDetails is { Count: 0 })
        {
            _logger.LogWarning(
                "Error options {ErrorSection} are empty, configure it so the exceptions do not return all {DefaultStatusCode}",
                ErrorsOptions.SectionName, DefaultStatusCode);
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (DcException dcEx)
        {
            await ProcessDcException(context, dcEx).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ProcessGenericException(context, ex).ConfigureAwait(false);
        }
    }

    private Task ProcessGenericException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Caught generic exception, returning {DefaultStatusCode}", DefaultStatusCode);
        return WriteResponse(context, new ErrorResponse
        {
            Code = DefaultStatusCode.ToString()
        }, DefaultStatusCode);
    }

    private Task ProcessDcException(HttpContext context, DcException ex)
    {
        _logger.LogError("DcException thrown: {ErrorCode} {DevMessage}", ex.ErrorCode, ex.Message);
        var response = new ErrorResponse
        {
            Code = ex.ErrorCode.ToString()
        };

        if (!_options.Value.ErrorCodeDetails.TryGetValue(ex.ErrorCode, out ErrorDetails? detail))
        {
            _logger.LogWarning("{ErrorCode} not configured, returning {DefaultStatusCode}", ex.ErrorCode,
                               DefaultStatusCode);

            return WriteResponse(context, response, DefaultStatusCode);
        }

        response.Message = detail.Message;
        response.Detail  = detail.Detail;
        return WriteResponse(context, response, detail.StatusCode);
    }


    private static Task WriteResponse(HttpContext context, ErrorResponse response, HttpStatusCode statusCode)
    {
        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = ContentType;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}