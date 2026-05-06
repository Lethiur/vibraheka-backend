namespace VibraHeka.Domain.Events.Models;

public class CreateEventResult
{
    public long EventID { get; set; } = 0L;
    public string JoinURL { get; set; } = string.Empty;
    public string StartUrl { get; set; } = string.Empty;
    public string EventPassword { get; set; } = string.Empty;
    public string RegistrationURL { get; set; } = string.Empty;
}
