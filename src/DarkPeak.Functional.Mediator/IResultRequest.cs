using Mediator;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// A request that returns <see cref="Result{T, TError}"/> with <typeparamref name="T"/> on success
/// or <see cref="Error"/> on failure. Use this instead of <see cref="IRequest{TResponse}"/>
/// to avoid specifying the full <c>Result&lt;T, Error&gt;</c> type.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public interface IResultRequest<T> : IRequest<Result<T, Error>>;

/// <summary>
/// A request that returns <see cref="Result{T, TError}"/> with <see cref="Unit"/> on success
/// or <see cref="Error"/> on failure. Use for requests that produce no meaningful return value.
/// </summary>
public interface IResultRequest : IRequest<Result<Unit, Error>>;
