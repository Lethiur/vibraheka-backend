using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

/// <summary>
/// Represents a command to update the content of an email template.
/// </summary>
/// <remarks>
/// This command is used to update the content of an existing email template by providing the
/// corresponding template identifier and the new content as a stream. It enforces administrative
/// permissions via the <see cref="IRequireAdmin"/> interface and specifies the operation result
/// as a functional outcome using the <see cref="Result{T}"/> type.
/// </remarks>
/// <param name="TemplateID">The unique identifier of the email template to be updated.</param>
/// <param name="TemplateStream">The stream containing the new content for the email template.</param>
public record UpdateTemplateContentCommand(string TemplateID, Stream TemplateStream) : IRequest<Result<Unit>>,  IRequireAdmin;
