namespace Sigapi.Common.Messaging;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest
{
    Task<ErrorOr<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}