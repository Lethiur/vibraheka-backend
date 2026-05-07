namespace VibraHeka.Domain.Events.Entities;

public partial record UserEventRegistration  : BaseAuditableEntity
{
    public String RegistrantID { get; set; } = string.Empty;
    public String EventID { get; set; } = string.Empty;
    public String UserID { get; set; } = string.Empty;
    public String JoinUrl { get; set; } = string.Empty;
}
