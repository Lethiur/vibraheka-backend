using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Users.Queries.AdminGetTherapists;

public record GetAllTherapistsQuery : IRequest<Result<List<UserEntity>>>, IRequireAdmin;
