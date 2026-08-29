using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Repositories;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.RecordingsAdapterTest;

public abstract class GenericRecordingsAdapterTest
{
    protected Mock<IRecordingRepository> RepositoryMock = default!;
    protected RecordingsAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RepositoryMock = new Mock<IRecordingRepository>();
        Adapter = new RecordingsAdapter(RepositoryMock.Object);
    }

    protected static RecordingEntity BuildDefaultRecordingEntity(string recordingId = "recording-unit-test-001") =>
        new()
        {
            RecordingID = recordingId,
            Name = "Test Recording Unit",
            Description = "Description for unit test adapter",
            IsActive = true,
            Tier = RecordingTier.Free,
            RecordingType = RecordingType.Meditacion,
        };
}
