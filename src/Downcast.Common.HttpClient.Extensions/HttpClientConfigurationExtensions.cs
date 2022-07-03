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
            var members = string.Join(",", result.MemberNames);
            return $"Members: [${members}] Error Message: {result.ErrorMessage}";
        });

        throw new ValidationException(
            $"Validation Error in {configurationSectionName} configuration section: \n" +
            $"{string.Join("\n", memberDetails)}");
    }

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