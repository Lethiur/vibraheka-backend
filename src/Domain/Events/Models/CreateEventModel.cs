namespace VibraHeka.Domain.Events.Models;

public class CreateEventModel
{
    public string Name { get; set; } = null!;
    public DateTimeOffset StartDate { get; set; }
    
    public int Duration { get; set; }
    
    public string EventPassword { get; set; } = string.Empty;
    
    public string EventTimezone { get; set; } = string.Empty;
}
