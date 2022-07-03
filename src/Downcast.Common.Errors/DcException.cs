namespace Downcast.Common.Errors;

public class DcException : Exception
{
    public ErrorCodes ErrorCode { get; }

    public DcException(ErrorCodes errorCode)
    {
        ErrorCode = errorCode;
    }

    public DcException(ErrorCodes errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}