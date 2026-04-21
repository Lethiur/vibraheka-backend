using NUnit.Framework;

namespace VibraHeka.Infrastructure.UnitTests.Mappers.RecordingEntityMapper;

public abstract class GenericRecordingEntityMapperTest
{
    protected VibraHeka.Infrastructure.Mappers.RecordingEntityMapper Mapper = default!;

    [SetUp]
    public void SetUp()
    {
        Mapper = new VibraHeka.Infrastructure.Mappers.RecordingEntityMapper();
    }
}
