using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Users.Ports.Out;

public interface UsersPort
{
    public Task<Result<UserEntity>> GetUserByEmailAsync(string email);
}
