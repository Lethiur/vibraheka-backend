using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;

/// <summary>
/// Represents a command to edit the name of an email template.
/// </summary>
/// <remarks>
/// This command requires administrative privileges to execute.
/// </remarks>
/// <param name="TemplateID">The unique identifier of the email template to be edited.</param>
/// <param name="NewTemplateName">The new name to be assigned to the email template.</param>
public record EditTemplateNameCommand(string TemplateID, string NewTemplateName) : IRequest<Result<Unit>>, IRequireAdmin;
