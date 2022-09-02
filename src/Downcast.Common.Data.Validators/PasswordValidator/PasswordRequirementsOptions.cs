using System.ComponentModel.DataAnnotations;

namespace Downcast.Common.Data.Validators.PasswordValidator;

public class PasswordRequirementsOptions : IValidatableObject
{
    public const string SectionName = "PasswordRequirementsOptions";

    [Required]
    public Dictionary<string, string> Requirements { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Requirements.Count is 0)
        {
            yield return new ValidationResult(
                $"At least one password requirement must be specified in {SectionName}.{nameof(Requirements)}",
                new[] { nameof(Requirements) });
        }
    }
}