using System.ComponentModel.DataAnnotations;

using Polly.Timeout;

namespace Downcast.Common.HttpClient.Extensions.Model;

public class TimeoutPolicyOptions
{
    public bool Enabled { get; set; }
    
    public TimeSpan TimeoutAfter { get; set; }

    [EnumDataType(typeof(TimeoutStrategy), ErrorMessage = "Invalid timeout strategy value")]
    public TimeoutStrategy TimeoutStrategy { get; set; }
}