using Mediator;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// Exception handler that catches unhandled exceptions thrown during message handling
/// and converts them into <see cref="Failure{T, TError}"/> with an <see cref="InternalError"/>.
/// This prevents exceptions from propagating out of the mediator pipeline, ensuring all
/// responses are expressed as <see cref="Result{T, TError}"/>.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
/// <typeparam name="T">The success type of the <see cref="Result{T, TError}"/> response.</typeparam>
public sealed class ResultExceptionHandler<TMessage, T>
    : MessageExceptionHandler<TMessage, Result<T, Error>>
    where TMessage : notnull, IMessage
{
    /// <inheritdoc />
    protected override ValueTask<ExceptionHandlingResult<Result<T, Error>>> Handle(
        TMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = new InternalError
        {
            Message = exception.Message,
            ExceptionType = exception.GetType().Name
        };

        return Handled(Result.Failure<T, Error>(error));
    }
}
