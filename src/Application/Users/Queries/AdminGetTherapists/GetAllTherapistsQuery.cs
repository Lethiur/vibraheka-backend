using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Admin.Queries.GetAllTherapists;

public record GetAllTherapistsQuery : IRequest<Result<IEnumerable<UserEntity>>>, IRequireAdmin;
