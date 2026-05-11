namespace VibraHeka.Domain.Events.Models;

public class RegisterAttendeeResult
{

    public long EventID { get; set; }

    public string RegistrantID { get; set; } = string.Empty;

    public string JoinURL { get; set; } = string.Empty;
}
