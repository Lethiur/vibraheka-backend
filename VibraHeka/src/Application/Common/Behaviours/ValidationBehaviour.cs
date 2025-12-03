using CSharpFunctionalExtensions;

namespace Microsoft.Extensions.DependencyInjection.Common.Behaviours;

public class ValidationBehaviour<TRequest,TResponse> : IPipelineBehavior<TRequest,TResponse> where TRequest : notnull 
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var errors = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(e => e != null)
                .ToList();

            if (errors.Count > 0)
            {
                var errorMessage = string.Join(" | ", errors.Select(e =>
                    $"{e.PropertyName}: {e.ErrorMessage}"
                ));

                throw new ValidationException(errorMessage);
            }
        }
        
        return await next(cancellationToken);
    }
}
