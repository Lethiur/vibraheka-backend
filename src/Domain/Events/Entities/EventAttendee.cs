using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Events.Entities;

[GenerateBuilderPattern]
public partial class EventAttendee
{
    public String AttendeeName { get; set; } = string.Empty;
    public String Email { get; set; } = string.Empty;
}
