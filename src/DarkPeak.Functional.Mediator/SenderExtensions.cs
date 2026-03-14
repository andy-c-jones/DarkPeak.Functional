using Mediator;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// Extension methods on <see cref="ISender"/> for sending messages that return
/// <see cref="Result{T, TError}"/>. These provide typed overloads that make
/// the <c>Result</c> return type explicit and improve discoverability.
/// </summary>
public static class SenderExtensions
{
    /// <summary>
    /// Sends a command that returns <see cref="Result{T, TError}"/>.
    /// </summary>
    public static ValueTask<Result<T, Error>> SendResult<T>(
        this ISender sender,
        IResultCommand<T> command,
        CancellationToken cancellationToken = default) =>
        sender.Send(command, cancellationToken);

    /// <summary>
    /// Sends a command that returns <see cref="Result{T, TError}"/> with <see cref="Unit"/>.
    /// </summary>
    public static ValueTask<Result<Unit, Error>> SendResult(
        this ISender sender,
        IResultCommand command,
        CancellationToken cancellationToken = default) =>
        sender.Send(command, cancellationToken);

    /// <summary>
    /// Sends a query that returns <see cref="Result{T, TError}"/>.
    /// </summary>
    public static ValueTask<Result<T, Error>> SendResult<T>(
        this ISender sender,
        IResultQuery<T> query,
        CancellationToken cancellationToken = default) =>
        sender.Send(query, cancellationToken);

    /// <summary>
    /// Sends a request that returns <see cref="Result{T, TError}"/>.
    /// </summary>
    public static ValueTask<Result<T, Error>> SendResult<T>(
        this ISender sender,
        IResultRequest<T> request,
        CancellationToken cancellationToken = default) =>
        sender.Send(request, cancellationToken);

    /// <summary>
    /// Sends a request that returns <see cref="Result{T, TError}"/> with <see cref="Unit"/>.
    /// </summary>
    public static ValueTask<Result<Unit, Error>> SendResult(
        this ISender sender,
        IResultRequest request,
        CancellationToken cancellationToken = default) =>
        sender.Send(request, cancellationToken);
}
