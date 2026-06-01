using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Catalog.Queries.GetRecordingDownloadUrl;

public abstract class GenericGetRecordingDownloadUrlTest
{
    protected const string UserId = "test-user-id-42";

    protected Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    protected Mock<IRecordingStoragePort> StoragePortMock = default!;
    protected Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    protected Mock<ISubscriptionService> SubscriptionServiceMock = default!;
    protected Mock<ILogger<GetRecordingDownloadUrlQueryHandler>> LoggerMock = default!;
    protected GetRecordingDownloadUrlQueryHandler Handler = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        StoragePortMock = new Mock<IRecordingStoragePort>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        SubscriptionServiceMock = new Mock<ISubscriptionService>();
        LoggerMock = new Mock<ILogger<GetRecordingDownloadUrlQueryHandler>>();

        CurrentUserServiceMock.Setup(s => s.UserId).Returns(UserId);

        Handler = new GetRecordingDownloadUrlQueryHandler(
            RegistryPortMock.Object,
            CurrentUserServiceMock.Object,
            SubscriptionServiceMock.Object,
            StoragePortMock.Object,
            LoggerMock.Object);
    }

    protected static GetRecordingDownloadUrlQuery BuildValidQuery() =>
        new(Guid.NewGuid().ToString());

    protected static GetRecordingDownloadUrlQuery BuildValidQuery(string recordingId) =>
        new(recordingId);

    protected static RecordingEntity BuildFreeRecordingEntity(string recordingId) =>
        new()
        {
            RecordingID = recordingId,
            Name = "Meditacion matutina",
            Description = "Una sesion de meditacion para empezar el dia",
            RecordingType = RecordingType.Meditacion,
            Tier = RecordingTier.Free,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
        };

    protected static RecordingEntity BuildPremiumRecordingEntity(string recordingId) =>
        new()
        {
            RecordingID = recordingId,
            Name = "Meditacion premium exclusiva",
            Description = "Sesion premium para suscriptores activos",
            RecordingType = RecordingType.Meditacion,
            Tier = RecordingTier.Premium,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
        };

    protected static SubscriptionEntity BuildActiveSubscriptionEntity() =>
        new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = UserId,
            SubscriptionStatus = SubscriptionStatus.Active,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
        };

    protected static SubscriptionEntity BuildInactiveSubscriptionEntity() =>
        new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = UserId,
            SubscriptionStatus = SubscriptionStatus.Inactive,
            StartDate = DateTimeOffset.UtcNow.AddDays(-60),
            EndDate = DateTimeOffset.UtcNow.AddDays(-30),
        };
}
