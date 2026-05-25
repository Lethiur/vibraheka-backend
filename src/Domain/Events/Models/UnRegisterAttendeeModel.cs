namespace VibraHeka.Domain.Events.Models;

public class UnRegisterAttendeeModel
{
    public string RegistrantID { get; set; } = string.Empty;
    public long EventID { get; set; }
}
