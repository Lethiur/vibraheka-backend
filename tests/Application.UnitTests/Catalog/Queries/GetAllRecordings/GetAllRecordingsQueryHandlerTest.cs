using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetAllRecordings;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Catalog.Queries.GetAllRecordings;

public abstract class GenericGetAllRecordingsQueryHandlerTest
{
    protected Mock<IRecordingRegistryPort> RegistryPortMock = default!;
    protected Mock<ILogger<GetAllRecordingsQueryHandler>> LoggerMock = default!;
    protected GetAllRecordingsQueryHandler Handler = default!;

    [SetUp]
    public void SetUp()
    {
        RegistryPortMock = new Mock<IRecordingRegistryPort>();
        LoggerMock = new Mock<ILogger<GetAllRecordingsQueryHandler>>();
        Handler = new GetAllRecordingsQueryHandler(RegistryPortMock.Object, LoggerMock.Object);
    }
}
