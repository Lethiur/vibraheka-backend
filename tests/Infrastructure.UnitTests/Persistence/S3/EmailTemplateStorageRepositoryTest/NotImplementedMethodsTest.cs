namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
public class NotImplementedMethodsTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    public void ShouldThrowNotImplementedForUnimplementedMethods()
    {
        Assert.That(
            async () => await Repository.DeleteTemplate("t1", TestCancellationToken),
            Throws.TypeOf<NotImplementedException>());
    }
}

