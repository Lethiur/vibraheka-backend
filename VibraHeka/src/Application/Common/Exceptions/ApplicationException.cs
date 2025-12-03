namespace VibraHeka.Infrastructure.Exceptions;

public class ApplicationException : Exception
{
    /// <summary>
    /// The error code associated with the error
    /// </summary>
    public string ErrorCode { get; private set; }
    
    /// <inheritdoc />
    protected ApplicationException(string errorCode, string errorMessage) : base(errorMessage)
    {
        ErrorCode = errorCode;
    } 
}
