using Moq;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.EmailTemplateStorageServiceTest;

public abstract class GenericEmailTemplateStorageServiceTest
{
    protected Mock<IEmailTemplateStorageRepository> RepositoryMock = default!;
    protected EmailTemplateStorageService Service = default!;
    protected CancellationToken TestCancellationToken;

    [SetUp]
    public void SetUp()
    {
        RepositoryMock = new Mock<IEmailTemplateStorageRepository>(MockBehavior.Strict);
        Service = new EmailTemplateStorageService(RepositoryMock.Object);
        TestCancellationToken = CancellationToken.None;
    }
}

