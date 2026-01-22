using System.Security.Claims;

namespace VibraHeka.Domain.Common.Interfaces;

/// <summary>
/// Represents a service to provide details about the currently authenticated user.
/// </summary>
/// <remarks>
/// This service typically retrieves information such as the user's unique identifier and claims
/// associated with the current HTTP context. It is commonly used to facilitate authorization
/// or user-specific behaviors in the application.
/// </remarks>
public interface ICurrentUserService
{
    /// Gets the identifier of the current user.
    /// This property provides the unique identifier for the user making the current request,
    /// or null if the user is not authenticated or their identifier cannot be determined.
    /// Typically, this property is used to track or validate actions performed in the
    /// context of a particular user.
    string? UserId { get; }
}
