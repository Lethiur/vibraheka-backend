using Infrastructure.AWS.DynamoDB.EmailTemplates.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;

namespace Infrastructure.AWS.DynamoDB.EmailTemplates.Mappers;

[Mapper]
public partial class EmailTemplateMapper
{
    public partial EmailTemplateEntity ToDomain(EmailTemplateDBModel model);
    
    public partial EmailTemplateDBModel FromDomain(EmailTemplateEntity entity);
}
