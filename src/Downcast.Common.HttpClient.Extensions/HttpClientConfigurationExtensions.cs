using System.ComponentModel.DataAnnotations;
using System.Net;

using Downcast.Common.HttpClient.Extensions.Model;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Downcast.Common.HttpClient.Extensions;

public static class HttpClientConfigurationExtensions
{
    /// <summary>
    /// Configures HttpClient with Polly policies.
    /// By default it tries to add a timeout policy, a circuit breaker policy and a retry policy.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName">Configuration section name from where
    /// the method will create <see cref="HttpClientOptions"/></param> option class.
    /// <returns></returns>
    public static IHttpClientBuilder ConfigureDowncastHttpClient(
        this IHttpClientBuilder builder,
        IConfiguration configuration,
        string configurationSectionName)
    {
        HttpClientOptions options = configuration.GetHttpClientOptions(configurationSectionName);
        return builder
            .ConfigureHttpClient(client => client.BaseAddress = options.ConnectionString)
            .AddCircuitBreakerPolicy(options.CircuitBreakerPolicyOptions)
            .AddTimeoutPolicy(options.TimeoutPolicyOptions)
            .AddRetryPolicy(options.RetryPolicyOptions);
    }

    /// <summary>
    /// Returns a validated <see cref="HttpClientOptions"/> options class.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configurationSectionName"></param>
    /// <returns>A validated instance of <see cref="HttpClientOptions"/></returns>
    /// <exception cref="ValidationException">If the configuration section does not correctly configure <see cref="HttpClientOptions"/></exception>
    public static HttpClientOptions GetHttpClientOptions(
        this IConfiguration configuration,
        string configurationSectionName)
    {
        HttpClientOptions? options = configuration.GetSection(configurationSectionName).Get<HttpClientOptions>();
        var context = new ValidationContext(options);
        var validations = new List<ValidationResult>();
        if (Validator.TryValidateObject(options, context, validations, true))
        {
            return options;
        }

        IEnumerable<string> memberDetails = validations.Select(result =>
        {
            var members = string.Join(", ", result.MemberNames);
            return $"Members: [${members}] Error Message: {result.ErrorMessage}";
        });

        throw new ValidationException(
            $"Validation Error in {configurationSectionName} configuration section: \n" +
            $"{string.Join("\n", memberDetails)}");
    }

    /// <summary>
    /// Adds a circuit breaker policy to the HttpClient. If the circuit breaker policy is is null, it will not do anything
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddCircuitBreakerPolicy(
        this IHttpClientBuilder builder,
        CircuitBreakerPolicyOptions? options)
    {
        if (options is not { Enabled: true })
        {
            return builder;
        }

        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(options.ExceptionsUntilBreak, options.BreakDuration);

        return builder.AddPolicyHandler(policy);
    }

    /// <summary>
    /// Adds a timeout policy to the HttpClient. If the timeout policy is is null, it will not do anything
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder builder, TimeoutPolicyOptions? options)
    {
        if (options is not { Enabled: true })
        {
            return builder;
        }

        IAsyncPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(
            options.TimeoutAfter,
            options.TimeoutStrategy
        );

        return builder.AddPolicyHandler(policy);
    }


    /// <summary>
    /// Adds a retry policy to the HttpClient. If the retry policy is is null, it will not do anything
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, RetryPolicyOptions? options)
    {
        if (options is not { Enabled: true })
        {
            return builder;
        }

        IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrResult(res => res.StatusCode is HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(options.RetryCount, GetRetryAttempt);

        return builder.AddPolicyHandler(policy);
    }

    private static TimeSpan GetRetryAttempt(int retryAttempt)
    {
        return TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt))
             + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
    }
}