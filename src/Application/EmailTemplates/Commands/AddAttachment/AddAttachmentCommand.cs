using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.AddAttachment;

/// <summary>
/// Represents a command to add an attachment to an email template.
/// </summary>
/// <remarks>
/// The command includes the necessary data to perform the attachment operation, such as the file stream of the attachment,
/// the unique identifier for the email template, and the name of the attachment.
/// </remarks>
public record AddAttachmentCommand(Stream FileStream, string TemplateId, string AttachmentName)
    : IRequest<Result<string>>, IRequireAdmin;
