using VibraHeka.Domain.Events.Enums;

namespace VibraHeka.Domain.Events.Models;

public class UpdateEventStatusModel
{
    public String EventID { get; set; } = string.Empty;

    public EventStatus Status { get; set; }
}
