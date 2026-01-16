using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class SettingsRepositoryTests
{
    private Mock<IAmazonSimpleSystemsManagement> _ssmClientMock;
    private SettingsRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _ssmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
        _repository = new SettingsRepository(_ssmClientMock.Object);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenParameterIsUpdatedCorrectly()
    {
        // Given: Un template de email válido y un cliente de AWS que responde correctamente
        string emailTemplate = "<html><body>Verify</body></html>";
        CancellationToken cancellationToken = CancellationToken.None;

        _ssmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), cancellationToken))
            .ReturnsAsync(new PutParameterResponse());

        // When: Se intenta actualizar el template en el repositorio
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

        // Then: El resultado debe indicar éxito y contener el valor Unit
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    public async Task ShouldReturnParameterLimitExceededWhenAwsThrowsParameterLimitExceededException()
    {
        // Given: El servicio de AWS alcanza el límite de parámetros permitidos
        string emailTemplate = "template";
        CancellationToken cancellationToken = CancellationToken.None;

        _ssmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), cancellationToken))
            .ThrowsAsync(new ParameterLimitExceededException("Limit reached"));

        // When: Se intenta realizar la actualización del parámetro
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

        // Then: Se debe capturar la excepción y retornar el error de negocio correspondiente
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterLimitExceeded));
    }

    [Test]
    public async Task ShouldReturnTooManyUpdatesWhenAwsThrowsTooManyUpdatesException()
    {
        // Given: El servicio de AWS recibe demasiadas actualizaciones en poco tiempo
        string emailTemplate = "template";
        CancellationToken cancellationToken = CancellationToken.None;

        _ssmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), cancellationToken))
            .ThrowsAsync(new TooManyUpdatesException("Too many updates"));

        // When: Se solicita la actualización del template
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

        // Then: El resultado debe fallar con el error de actualización excesiva
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.TooManyUpdates));
    }

    [Test]
    public async Task ShouldReturnAccessDeniedWhenAwsThrowsAmazonSimpleSystemsManagementException()
    {
        // Given: Un error general de AWS SSM que suele estar relacionado con permisos o conectividad
        string emailTemplate = "template";
        CancellationToken cancellationToken = CancellationToken.None;

        _ssmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), cancellationToken))
            .ThrowsAsync(new AmazonSimpleSystemsManagementException("Access Denied"));

        // When: El repositorio intenta comunicarse con AWS
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

        // Then: Se mapea el error genérico de SSM a una denegación de acceso en la infraestructura
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.AccessDenied));
    }

    [Test]
    public async Task ShouldReturnUnexpectedErrorWhenGeneralExceptionIsThrown()
    {
        // Given: Una excepción no controlada o error de sistema inesperado
        string emailTemplate = "template";
        CancellationToken cancellationToken = CancellationToken.None;

        _ssmClientMock.Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), cancellationToken))
            .ThrowsAsync(new Exception("Generic error"));

        // When: Ocurre el fallo durante la ejecución del método
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

        // Then: El repositorio debe asegurar que se retorne un error inesperado genérico
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.UnexpectedError));
    }
}
