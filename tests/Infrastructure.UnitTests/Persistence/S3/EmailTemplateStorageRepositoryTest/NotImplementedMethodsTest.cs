namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
public class NotImplementedMethodsTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    public void ShouldThrowNotImplementedForUnimplementedMethods()
    {
        Assert.That(
            async () => await Repository.GetEmailTemplatePath("t1", TestCancellationToken),
            Throws.TypeOf<NotImplementedException>());

        Assert.That(
            async () => await Repository.GetAuthorizationStringForTemplateRead("t1", TestCancellationToken),
            Throws.TypeOf<NotImplementedException>());

        Assert.That(
            async () => await Repository.GetAuthorizationStringForTemplateWrite("t1", TestCancellationToken),
            Throws.TypeOf<NotImplementedException>());

        Assert.That(
            async () => await Repository.DeleteTemplate("t1", TestCancellationToken),
            Throws.TypeOf<NotImplementedException>());
    }
}

