using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.EmailTemplates;

namespace VibraHeka.Web.Controllers.EmailTemplates;

[Mapper]
public partial class EmailTemplateMapper
{
    [MapperIgnoreTarget(nameof(SimpleEmailTemplateDTO.AdditionalProperties))]
    [MapperIgnoreSource(nameof(EmailEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(EmailEntity.LastModifiedBy))]
    [MapperIgnoreSource(nameof(EmailEntity.Path))]
    [MapperIgnoreSource(nameof(EmailEntity.Attachments))]
    public partial SimpleEmailTemplateDTO ToDTO(EmailEntity entity);
    
    public partial Guid ToGuid(string id);
    
    public partial Uri ToUri(string url);
}
