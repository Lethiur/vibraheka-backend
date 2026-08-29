using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Queries.GetProfile;

public record GetUserProfileQuery(string UserID) : IRequest<Result<UserEntity>>;
