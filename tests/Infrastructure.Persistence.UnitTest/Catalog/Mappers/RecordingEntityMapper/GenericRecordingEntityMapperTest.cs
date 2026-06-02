namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Mappers.RecordingEntityMapper;

public abstract class GenericRecordingEntityMapperTest
{
    protected global::Infrastructure.Persistence.Catalog.Mappers.RecordingEntityMapper Mapper = default!;

    [SetUp]
    public void SetUp()
    {
        Mapper = new global::Infrastructure.Persistence.Catalog.Mappers.RecordingEntityMapper();
    }
}
