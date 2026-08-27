using CSharpFunctionalExtensions;
using VibraHeka.Domain.Models.DTO.User;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public record GetUserProfileQuery(string UserID) : IRequest<Result<UserDTO>>;
