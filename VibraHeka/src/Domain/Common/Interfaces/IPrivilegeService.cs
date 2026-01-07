using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces;

public interface IPrivilegeService
{
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role);

    public Task<Result<bool>> CanEditResource(string userId, string resourceId);
}
    
