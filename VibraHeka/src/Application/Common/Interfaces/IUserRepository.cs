using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
}
