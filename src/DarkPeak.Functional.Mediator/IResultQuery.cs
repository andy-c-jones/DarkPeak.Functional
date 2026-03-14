using Mediator;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// A query that returns <see cref="Result{T, TError}"/> with <typeparamref name="T"/> on success
/// or <see cref="Error"/> on failure. Use this instead of <see cref="IQuery{TResponse}"/>
/// to avoid specifying the full <c>Result&lt;T, Error&gt;</c> type.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public interface IResultQuery<T> : IQuery<Result<T, Error>>;
