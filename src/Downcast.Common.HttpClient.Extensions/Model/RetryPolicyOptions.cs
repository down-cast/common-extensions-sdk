namespace Downcast.Common.HttpClient.Extensions.Model;

public class RetryPolicyOptions
{
    public bool Enabled { get; set; }
    public int RetryCount { get; set; }
}