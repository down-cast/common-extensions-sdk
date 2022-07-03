namespace Downcast.Common.Errors;

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