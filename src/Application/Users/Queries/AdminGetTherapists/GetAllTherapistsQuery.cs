using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Admin.Queries.GetAllTherapists;

public record GetAllTherapistsQuery : IRequest<Result<IEnumerable<User>>>;
