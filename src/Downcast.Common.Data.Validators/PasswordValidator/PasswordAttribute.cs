using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using Downcast.Common.Errors;

using Microsoft.Extensions.Options;

namespace Downcast.Common.Data.Validators.PasswordValidator;

/// <summary>
/// Validates that a password is strong enough.
/// Requires <see cref="PasswordRequirementsOptions"/> to be registered in the DI container.
/// </summary>
public class PasswordAttribute : RequiredAttribute
{
    private readonly bool _enforceStrength;

    public PasswordAttribute(bool enforceStrength)
    {
        _enforceStrength = enforceStrength;
    }

    public override bool RequiresValidationContext => true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string { Length: > 0 } password)
        {
            return new ValidationResult("Password cannot be null or empty");
        }

        if (_enforceStrength is false)
        {
            return ValidationResult.Success;
        }

        Dictionary<string, string> requirements = GetPasswordRequirements(validationContext);

        return ValidateRegexRequirements(password, requirements);
    }

    private static Dictionary<string, string> GetPasswordRequirements(IServiceProvider validationContext)
    {
        object? requirements = validationContext.GetService(typeof(IOptions<PasswordRequirementsOptions>));
        if (requirements is not IOptions<PasswordRequirementsOptions> { Value.Requirements.Count: > 0 } options)
        {
            throw new DcException(ErrorCodes.InternalServerError, "Password requirements not configured");
        }

        return options.Value.Requirements;
    }

    private static ValidationResult? ValidateRegexRequirements(
        string password,
        Dictionary<string, string> requirements)
    {
        List<string> failedRequirements = new();
        foreach ((string requirementName, string regex) in requirements)
        {
            if (Regex.IsMatch(password, regex))
            {
                continue;
            }

            failedRequirements.Add(requirementName);
        }

        return failedRequirements.Any()
            ? new ValidationResult($"Password does not meet requirements: {string.Join(", ", failedRequirements)}")
            : ValidationResult.Success;
    }
}