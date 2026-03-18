using Amazon.SimpleSystemsManagement;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

public abstract class GenericSettingsRepositoryTest
{
    protected Mock<IAmazonSimpleSystemsManagement> SsmClientMock;
    protected Mock<ILogger<SettingsRepository>> LoggerMock;
    protected SettingsRepository Repository;
    protected AWSConfig config;
    protected const string VerificationParameterName = "/TEST/VerificationEmailTemplate";
    protected const string RecoverPasswordParameterName = "/TEST/RecoverPasswordEmailTemplate";
    protected const string UserWelcomeParameterName = "/TEST/UserWelcomeEmailTemplate";
    protected const string SubscriptionThankYouParameterName = "/TEST/SubscriptionThankYouEmailTemplate";
    protected const string TrialEndingSoonParameterName = "/TEST/TrialEndingSoonEmailTemplate";
    protected const string PasswordChangedParameterName = "/TEST/PasswordChangedEmailTemplate";

    [SetUp]
    public void SetUp()
    {
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("mock"));
        config = new AWSConfig() { SettingsNameSpace = "TEST" };
        SsmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
        LoggerMock = new Mock<ILogger<SettingsRepository>>();
        Repository = new SettingsRepository(SsmClientMock.Object, config, LoggerMock.Object);
    }
}
