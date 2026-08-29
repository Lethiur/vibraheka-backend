using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(string Email, string FirstName, string MiddleName, string LastName, string PhoneNumber, string Bio, string ProfilePictureUrl, string TimezoneID) : IRequest<Result<Unit>>;
