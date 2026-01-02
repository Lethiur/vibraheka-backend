using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    /// <remarks>
    /// This property retrieves the user ID from the claims of the current HTTP context's principal user.
    /// It corresponds to the "sub" claim typically used in authentication systems to represent the user's identifier.
    /// Returns null if no user is authenticated or if the claim is not present.
    /// </remarks>
    public string? UserId
    {
        get
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    } 
    public ClaimsPrincipal? Principals => httpContextAccessor.HttpContext?.User;
}
