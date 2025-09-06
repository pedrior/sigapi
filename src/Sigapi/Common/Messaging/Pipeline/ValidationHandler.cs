using FluentValidation.Results;

namespace Sigapi.Common.Messaging.Pipeline;

public sealed class ValidationHandler<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    IEnumerable<IValidator<TRequest>> validators) : IRequestHandler<TRequest, TResponse> where TRequest : IRequest
{
    public async Task<ErrorOr<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var validatorArray = validators as IValidator<TRequest>[] ?? validators.ToArray();
        var failures = await ValidateAsync(request, validatorArray, cancellationToken);

        return failures.Count is 0
            ? await inner.HandleAsync(request, cancellationToken)
            : MapValidationFailuresToErrors(failures)
                .ToArray();
    }

    private static async Task<IList<ValidationFailure>> ValidateAsync(
        TRequest request,
        IValidator<TRequest>[] validators,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(validator =>
            validator.ValidateAsync(context, cancellationToken)));

        return results.SelectMany(result => result.Errors)
            .ToList();
    }

    private static IEnumerable<Error> MapValidationFailuresToErrors(IList<ValidationFailure> failures) =>
        failures.Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage));
}