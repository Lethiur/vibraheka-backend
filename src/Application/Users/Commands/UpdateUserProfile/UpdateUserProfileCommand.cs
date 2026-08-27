using CSharpFunctionalExtensions;
using VibraHeka.Domain.Models.DTO.User;

namespace VibraHeka.Application.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(UserDTO NewUserData) : IRequest<Result<Unit>>;
