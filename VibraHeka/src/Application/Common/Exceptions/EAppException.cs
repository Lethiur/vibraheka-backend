namespace VibraHeka.Application.Common.Exceptions;

public class EAppException : Exception
{
    /// <summary>
    /// The error code associated with the error
    /// </summary>
    public string ErrorCode { get; private set; }
    
    /// <inheritdoc />
    protected EAppException(string errorCode, string errorMessage) : base(errorMessage)
    {
        ErrorCode = errorCode;
    } 
}
