namespace Infrastructure.Rest.Client.Stripe.Models;

public class RegisterCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
}
