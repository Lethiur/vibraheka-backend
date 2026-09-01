using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Settings.Controllers;

namespace VibraHeka.Web.Controllers.Settings;

[Mapper]
public partial class SettingsMapper
{
    public partial ChangeTemplateForActionCommand ToCommand(UpdateTemplateForActionRequest request);
    
    [MapProperty(nameof(TemplateForActionEntity.TemplateID), nameof(TemplateForActionDTO.Id))]
    public partial TemplateForActionDTO ToDto(TemplateForActionEntity entity);
}
