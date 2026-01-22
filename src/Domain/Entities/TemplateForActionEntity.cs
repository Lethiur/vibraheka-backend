using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

/// <summary>
/// Represents a template entity associated with a specific action type.
/// </summary>
public class TemplateForActionEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the template associated with a specific action.
    /// This property is used to link an action type to its corresponding template in the system.
    /// </summary>
    public string TemplateID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of action that this template is associated with.
    /// This property defines the specific action category, such as user registration,
    /// verification, or password reset, that determines the behavior or content of the template.
    /// </summary>
    public ActionType ActionType { get; set; } = ActionType.PasswordReset;
}
