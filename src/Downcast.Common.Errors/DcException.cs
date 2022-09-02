namespace Downcast.Common.Errors;

/// <summary>
/// Generic Exception that allows for a message and an <see cref="ErrorCodes"/> to be set
/// </summary>
public class DcException : Exception
{
    public string ErrorCode { get; }

    public DcException(ErrorCodes errorCode)
    {
        ErrorCode = errorCode.ToString();
    }

    public DcException(ErrorCodes errorCode, string message) : base(message)
    {
        ErrorCode = errorCode.ToString();
    }
}