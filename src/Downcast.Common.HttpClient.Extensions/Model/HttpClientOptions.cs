using System.ComponentModel.DataAnnotations;

namespace Downcast.Common.HttpClient.Extensions.Model;

public class HttpClientOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Http client name must be unique and non empty")]
    public string ClientName { get; set; } = null!;

    [Required(ErrorMessage = $"Please provide a valid uri for {nameof(ConnectionString)})")]
    public Uri ConnectionString { get; set; } = null!;
    
    public CircuitBreakerPolicyOptions? CircuitBreakerPolicyOptions { get; set; }
    public TimeoutPolicyOptions? TimeoutPolicyOptions { get; set; }
    public RetryPolicyOptions? RetryPolicyOptions { get; set; }
}