namespace VibraHeka.Application.Common.Interfaces;

public interface ICognitoService
{
    Task<string> RegisterUserAsync(string email, string password, string fullName);
}
