using FluentValidation;
using MediatR;
using NUnit.Framework;
using VibraHeka.Application.Common.Behaviours;

namespace VibraHeka.Application.UnitTests.Common.Behaviours;

public record TestValidationRequest(string Name) : IRequest<string>;

public class TestValidationRequestValidator : AbstractValidator<TestValidationRequest>
{
    public TestValidationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("NAME_REQUIRED");
    }
}

[TestFixture]
public class ValidationBehaviourTest
{
    [Test]
    public async Task ShouldCallNextWhenNoValidatorsAreRegistered()
    {
        // Given
        ValidationBehaviour<TestValidationRequest, string> behaviour = new([]);
        TestValidationRequest request = new("any");

        // When
        string result = await behaviour.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Then
        Assert.That(result, Is.EqualTo("ok"));
    }

    [Test]
    public void ShouldThrowValidationExceptionWhenValidationFails()
    {
        // Given
        ValidationBehaviour<TestValidationRequest, string> behaviour = new([new TestValidationRequestValidator()]);
        TestValidationRequest request = new(string.Empty);

        // When
        TestDelegate action = () => behaviour.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None).GetAwaiter().GetResult();

        // Then
        ValidationException ex = Assert.Throws<ValidationException>(action)!;
        Assert.That(ex.Message, Does.Contain("NAME_REQUIRED"));
    }
}
