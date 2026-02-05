using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Admin.Queries.GetAllTherapists;

/// <summary>
/// Handles the execution of the <see cref="GetAllTherapistsQuery"/>, retrieving a list of all users with the role of a therapist.
/// </summary>
/// <remarks>
/// This request handler orchestrates the retrieval of therapist data from the system, leveraging services to validate
/// the current user's permissions and to access the user repository. The result is a list of users who are registered as therapists.
/// </remarks>
/// <param name="CurrentUserService">
/// Provides information about the current authenticated user, such as user ID and claims.
/// </param>
/// <param name="privilegeService">
/// Service used to validate the user's privileges, ensuring they have the necessary permissions to perform the query.
/// </param>
/// <param name="Repository">
/// Repository to interact with the persistence layer, allowing access to user data.
/// </param>
/// <returns>
/// A task that represents the asynchronous operation, which contains a <see cref="Result{T}"/> object
/// holding a list of <see cref="UserEntity"/> entities or an error if the operation fails.
/// </returns>
public class GetAllTherapistsQueryHandler(
    IUserRepository Repository) : IRequestHandler<GetAllTherapistsQuery, Result<IEnumerable<UserEntity>>>
{
    public async Task<Result<IEnumerable<UserEntity>>> Handle(GetAllTherapistsQuery request,
        CancellationToken cancellationToken)
    {
        return await Repository.GetByRoleAsync(UserRole.Therapist);
    }
}
