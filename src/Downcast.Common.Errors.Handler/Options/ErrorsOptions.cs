namespace Downcast.Common.Errors.Handler.Options;

public class ErrorsOptions
{
    public const string SectionName = "ErrorsOptions";
    public Dictionary<string, ErrorDetails> ErrorCodeDetails { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
}
