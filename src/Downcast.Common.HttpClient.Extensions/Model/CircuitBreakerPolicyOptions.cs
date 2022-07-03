namespace Downcast.Common.HttpClient.Extensions.Model;

public class CircuitBreakerPolicyOptions
{
    public bool Enabled { get; set; }
    public int ExceptionsUntilBreak { get; set; }
    public TimeSpan BreakDuration { get; set; }
}