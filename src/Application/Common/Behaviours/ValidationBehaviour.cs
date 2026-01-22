using FluentValidation.Results;

namespace VibraHeka.Application.Common.Behaviours;

/// <summary>
/// Represents a pipeline behavior for request validation in the application.
/// This behavior ensures that any incoming request of type <typeparamref name="TRequest"/>
/// is validated against the associated validators before executing the next step in the pipeline.
/// If any validation errors are detected, a <see cref="ValidationException"/> is thrown.
/// </summary>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
/// <typeparam name="TResponse">The type of the response expected from handling the request.</typeparam>
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Holds a collection of validators for the request type <typeparamref name="TRequest"/>.
    /// These validators are utilized to evaluate the validity of incoming requests
    /// before proceeding with the execution of the corresponding pipeline behavior.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <summary>
    /// Handles the validation of the incoming request and forwards it to the next behavior in the pipeline.
    /// Validates the request using a collection of validators, and if validation errors are found,
    /// throws a <see cref="ValidationException"/> containing the error details.
    /// </summary>
    /// <param name="request">The request object to be processed and validated.</param>
    /// <param name="next">The delegate representing the next step in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>The response produced by the next handler in the pipeline, if the request passes validation.</returns>
    /// <exception cref="ValidationException">Thrown when validation errors are detected in the request.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

            List<ValidationFailure> errors = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(e => e != null)
                .ToList();

            if (errors.Count > 0)
            {
                string errorMessage = string.Join(" | ", errors.Select(e => $"{e.ErrorMessage}"
                ));

                throw new ValidationException(errorMessage);
            }
        }
        return await next(cancellationToken);
    }
}
