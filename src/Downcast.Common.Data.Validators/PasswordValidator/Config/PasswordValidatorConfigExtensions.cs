using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Downcast.Common.Data.Validators.PasswordValidator.Config;

public static class PasswordValidatorConfigExtensions
{
    /// <summary>
    /// Adds an IOptions of <see cref="PasswordRequirementsOptions"/> to the services, allowing <see cref="PasswordAttribute"/>
    /// to work correctly.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddPasswordValidatorOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PasswordRequirementsOptions>()
            .Bind(configuration.GetSection(PasswordRequirementsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}