namespace VibraHeka.Domain.Entities;

/// <summary>
/// Represents a generic response entity that encapsulates the result of an operation.
/// </summary>
public class ResponseEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation associated with this response was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a code that represents the specific error encountered during the operation, if any.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the content associated with the response, providing additional data or information about the operation.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="ResponseEntity"/> representing a successful operation
    /// with the specified content.
    /// </summary>
    /// <param name="content">The content associated with the successful operation.</param>
    /// <returns>A new <see cref="ResponseEntity"/> instance with the provided content set
    /// and success status set to true.</returns>
    public static ResponseEntity FromSuccess(object content)
    {
        return new ResponseEntity { Success = true, ErrorCode = null, Content = content };
    }

    /// <summary>
    /// Creates a new instance of <see cref="ResponseEntity"/> representing an unsuccessful operation
    /// with the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code that represents the reason for the failure.</param>
    /// <returns>A new <see cref="ResponseEntity"/> instance with the provided error code set
    /// and success status set to false.</returns>
    public static ResponseEntity FromError(string errorCode)
    {
        return new ResponseEntity { Success = false, ErrorCode = errorCode, Content = null };
    }

    #if DEBUG
    public T? GetContentAs<T>()
    {
        return Content == null ? default : (T)Content;
    }
    #endif

}
