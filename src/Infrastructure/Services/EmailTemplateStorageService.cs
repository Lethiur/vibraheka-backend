using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Infrastructure.Services;

public class EmailTemplateStorageService(IEmailTemplateStorageRepository repository) : IEmailTemplateStorageService
{
    private readonly IEmailTemplateStorageRepository _repository = repository;
    
    public async Task<Result<string>> SaveTemplate(string templateID, Stream templateStream, CancellationToken cancellationToken)
    {
        await _repository.SaveTemplate(templateID, templateStream, cancellationToken);
        return templateID;
    }
}
