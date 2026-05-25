using Infrastructure.Rest.Client.Zoom.Enums;
using Infrastructure.Rest.Client.Zoom.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.Zoom.Mappers;

[Mapper]
public partial class ZoomMeetingMapper
{
    [MapProperty(nameof(CreateEventModel.Name), nameof(ZoomCreateMeetingRequest.Topic))]
    [MapProperty(nameof(CreateEventModel.StartDate), nameof(ZoomCreateMeetingRequest.StartTimeUtc))]
    [MapProperty(nameof(CreateEventModel.Duration), nameof(ZoomCreateMeetingRequest.DurationInMinutes))]
    [MapProperty(nameof(CreateEventModel.EventPassword), nameof(ZoomCreateMeetingRequest.Password))]
    [MapProperty(nameof(CreateEventModel.EventTimezone), nameof(ZoomCreateMeetingRequest.Timezone))]
    [MapValue(nameof(ZoomCreateMeetingRequest.Type), MeetingType.Scheduled)]
    [MapValue(nameof(ZoomCreateMeetingRequest.JoinBeforeHost), true)]
    [MapValue(nameof(ZoomCreateMeetingRequest.WaitingRoomEnabled), true)]
    [MapValue(nameof(ZoomCreateMeetingRequest.SendZoomEmail), true)]
    public partial ZoomCreateMeetingRequest ToZoomRequest(CreateEventModel model);


    [MapProperty(nameof(ZoomCreateMeetingResponse.Id), nameof(CreateEventResult.EventID))]
    [MapProperty(nameof(ZoomCreateMeetingResponse.JoinUrl), nameof(CreateEventResult.JoinURL))]
    [MapProperty(nameof(ZoomCreateMeetingResponse.StartUrl), nameof(CreateEventResult.StartUrl))]
    [MapProperty(nameof(ZoomCreateMeetingResponse.Password), nameof(CreateEventResult.EventPassword))]
    [MapProperty(nameof(ZoomCreateMeetingResponse.RegistrationUrl), nameof(CreateEventResult.RegistrationURL))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Uuid))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.EncryptedPassword))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Settings))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.HostId))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.HostEmail))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Topic))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Type))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Status))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.Timezone))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.StartTimeUtc))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.DurationInMinutes))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.H323Password))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.CreatedAtUtc))]
    [MapperIgnoreSource(nameof(ZoomCreateMeetingResponse.PstnPassword))]
    public partial CreateEventResult CreateMeetingResponseToDomain(ZoomCreateMeetingResponse response);

    [MapProperty(nameof(UnRegisterAttendeeModel.EventID), nameof(ZoomUnRegisterRegistrantRequest.MeetingID))]
    [MapProperty(nameof(UnRegisterAttendeeModel.RegistrantID), nameof(ZoomUnRegisterRegistrantRequest.RegistrantID))]
    public partial ZoomUnRegisterRegistrantRequest ToZoomUnRegisterRegistrantRequest(UnRegisterAttendeeModel model);

    [MapProperty(nameof(RegisterAttendeeModel.EventID), nameof(ZoomRegisterRegistrantRequest.MeetingID))]
    [MapProperty(nameof(RegisterAttendeeModel.RegistrantEmail), nameof(ZoomRegisterRegistrantRequest.Email))]
    [MapProperty(nameof(RegisterAttendeeModel.RegistrantName), nameof(ZoomRegisterRegistrantRequest.FirstName))]
    [MapProperty(nameof(RegisterAttendeeModel.RegistrantLastName), nameof(ZoomRegisterRegistrantRequest.LastName))]
    [MapValue(nameof(ZoomRegisterRegistrantRequest.AutoApprove), true)]
    [MapperIgnoreSource(nameof(RegisterAttendeeModel.UserID))]
    public partial ZoomRegisterRegistrantRequest ToZoomRequest(RegisterAttendeeModel model);

    [MapProperty(nameof(ZoomCreateRegistrantResposne.MeetingId), nameof(RegisterAttendeeResult.EventID))]
    [MapProperty(nameof(ZoomCreateRegistrantResposne.RegistrantId), nameof(RegisterAttendeeResult.RegistrantID))]
    [MapProperty(nameof(ZoomCreateRegistrantResposne.JoinUrl), nameof(RegisterAttendeeResult.JoinURL))]
    [MapperIgnoreSource(nameof(ZoomCreateRegistrantResposne.Topic))]
    [MapperIgnoreSource(nameof(ZoomCreateRegistrantResposne.StartTimeUtc))]
    public partial RegisterAttendeeResult ZoomRegisterRegistrantResponseToDomain(ZoomCreateRegistrantResposne response);
}
