namespace VibraHeka.Domain.Events.Models;

public class RegisterAttendeeModel
{
    public long EventID { get; set; }
    public string RegistrantEmail { get; set; } = string.Empty;
    public string RegistrantName { get; set; } = string.Empty;
    public string RegistrantLastName { get; set; } = string.Empty;

}
