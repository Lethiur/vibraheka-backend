using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;

/// <summary>
/// Represents a command to change the template associated with a specific action type.
/// </summary>
/// <param name="TemplateID">
/// The unique identifier of the template to be associated with the specified action type.
/// </param>
/// <param name="Action">
/// The action type for which the template will be updated. This defines the context
/// or scenario in which the template will be used, such as user registration or password reset.
/// </param>
/// <remarks>
/// This command is typically used to update configuration or settings
/// within the application that tie templates to specific user actions.
/// </remarks>
public record ChangeTemplateForActionCommand(string TemplateID, ActionType Action) : IRequest<Result<Unit>>;
