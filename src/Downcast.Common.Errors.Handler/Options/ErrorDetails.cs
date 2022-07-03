using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Downcast.Common.Errors.Handler.Options;

public class ErrorDetails : IValidatableObject
{
    public string? Message { get; set; }
    public string? Detail { get; set; }

    [EnumDataType(typeof(HttpStatusCode), ErrorMessage = "Invalid Http Status Code")]
    public HttpStatusCode StatusCode { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StatusCode == default)
        {
            yield return new ValidationResult($"Status code {StatusCode} is not valid",
                                              new[] { nameof(StatusCode) });
        }
    }
    
}