namespace Downcast.Common.Errors.Handler;

internal class ErrorResponse
{
    public string Code { get; set; } = null!;
    public string? Message { get; set; }
    public string? Detail { get; set; }
}