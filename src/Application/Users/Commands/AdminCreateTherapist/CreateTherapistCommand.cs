using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.Users.Commands.AdminCreateTherapist;

/// <summary>
/// Represents a command used to create a new therapist within the system.
/// </summary>
/// <remarks>
/// This command encapsulates all necessary data required to create a therapist,
/// such as their email and name. It is processed by a handler to execute the creation logic.
/// </remarks>
public record CreateTherapistCommand(UserDTO TherapistData) : IRequest<Result<string>>, IRequireAdmin;
