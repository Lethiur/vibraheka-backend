using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Catalog.Commands.AdminAddRecording;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Web.Catalog.Recordings.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.Recordings;

[Mapper]
public partial class RecordingMapper
{
    [MapProperty(nameof(CreateRecordingRequest.Currency), nameof(AdminAddRecordingCommand.CurrencyCode))]
    public partial AdminAddRecordingCommand ToAdminCommand(CreateRecordingRequest request);
    
    [MapProperty(nameof(AddRecordingResult.RecordingId), nameof(CreateRecordingResponse.Id))]
    public partial CreateRecordingResponse ToResponse(AddRecordingResult result);
    
    [MapProperty(nameof(RecordingEntity.RecordingID), nameof(ExtendedRecordingDTO.Id))]
    [MapProperty(nameof(RecordingEntity.Created), nameof(ExtendedRecordingDTO.CreatedAt))]
    [MapProperty(nameof(RecordingEntity.LastModified), nameof(ExtendedRecordingDTO.LastModifiedAt))]
    [MapProperty(nameof(RecordingEntity.RecordingType), nameof(ExtendedRecordingDTO.Type))]
    [MapperIgnoreSource(nameof(RecordingEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(RecordingEntity.LastModifiedBy))]
    [MapperIgnoreSource(nameof(RecordingEntity.ID))]
    public partial ExtendedRecordingDTO ToAdminResponse(RecordingEntity entity);
    
    [MapProperty(nameof(RecordingEntity.RecordingID), nameof(ExtendedRecordingDTO.Id))]
    [MapProperty(nameof(RecordingEntity.RecordingType), nameof(ExtendedRecordingDTO.Type))]
    [MapperIgnoreSource(nameof(RecordingEntity.ID))]
    [MapperIgnoreSource(nameof(RecordingEntity.IsActive))]
    [MapperIgnoreSource(nameof(RecordingEntity.Created))]
    [MapperIgnoreSource(nameof(RecordingEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(RecordingEntity.LastModified))]
    [MapperIgnoreSource(nameof(RecordingEntity.LastModifiedBy))]
    public partial RecordingDTO ToResponse(RecordingEntity entity);
}
