using Mediator;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// Pipeline behavior that validates messages implementing <see cref="IValidate"/> before
/// the handler executes. If validation fails, the pipeline short-circuits and returns
/// a <see cref="Failure{T, TError}"/> containing the <see cref="ValidationError"/>.
/// </summary>
/// <typeparam name="TMessage">The message type, which must implement <see cref="IValidate"/>.</typeparam>
/// <typeparam name="T">The success type of the <see cref="Result{T, TError}"/> response.</typeparam>
public sealed class ResultValidationBehavior<TMessage, T>
    : IPipelineBehavior<TMessage, Result<T, Error>>
    where TMessage : notnull, IMessage, IValidate
{
    /// <inheritdoc />
    public ValueTask<Result<T, Error>> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, Result<T, Error>> next,
        CancellationToken cancellationToken)
    {
        if (!message.IsValid(out var error))
            return new ValueTask<Result<T, Error>>(Result.Failure<T, Error>(error));

        return next(message, cancellationToken);
    }
}
