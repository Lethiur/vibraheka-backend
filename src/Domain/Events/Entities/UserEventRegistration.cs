using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Events.Entities;


[GenerateBuilderPattern]
public partial class UserEventRegistration
{
    public String RegistrantID { get; set; } = string.Empty;
    public String EventID { get; set; } = string.Empty;
    public String UserID { get; set; } = string.Empty;
    public String JoinUrl { get; set; } = string.Empty;
}
