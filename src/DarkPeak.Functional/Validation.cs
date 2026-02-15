namespace DarkPeak.Functional;

/// <summary>
/// Represents the result of a validation that can either succeed with a value
/// or fail with one or more errors. Unlike Result, multiple Validation values
/// can be combined to accumulate all errors via Apply.
/// </summary>
/// <typeparam name="T">The type of the validated value.</typeparam>
/// <typeparam name="TError">The type of the error, must inherit from Error.</typeparam>
public abstract record Validation<T, TError> where TError : Error
{
    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public abstract bool IsValid { get; }

    /// <summary>
    /// Gets a value indicating whether the validation failed.
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Matches the validation to one of two functions based on success or failure.
    /// </summary>
    public abstract TResult Match<TResult>(Func<T, TResult> valid, Func<IReadOnlyList<TError>, TResult> invalid);

    /// <summary>
    /// Asynchronously matches the validation to one of two functions.
    /// </summary>
    public abstract Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> valid, Func<IReadOnlyList<TError>, Task<TResult>> invalid);

    /// <summary>
    /// Transforms the value inside the validation using the provided function.
    /// </summary>
    public abstract Validation<TResult, TError> Map<TResult>(Func<T, TResult> mapper);

    /// <summary>
    /// Asynchronously transforms the value inside the validation.
    /// </summary>
    public abstract Task<Validation<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper);

    /// <summary>
    /// Binds the validation to a function that returns another validation (short-circuits on failure).
    /// Use Apply instead if you want to accumulate errors.
    /// </summary>
    public abstract Validation<TResult, TError> Bind<TResult>(Func<T, Validation<TResult, TError>> binder);

    /// <summary>
    /// Asynchronously binds the validation (short-circuits on failure).
    /// </summary>
    public abstract Task<Validation<TResult, TError>> BindAsync<TResult>(
        Func<T, Task<Validation<TResult, TError>>> binder);

    /// <summary>
    /// Executes an action if the validation succeeded (side effect).
    /// </summary>
    public abstract Validation<T, TError> Tap(Action<T> action);

    /// <summary>
    /// Executes an action if the validation failed (side effect).
    /// </summary>
    public abstract Validation<T, TError> TapInvalid(Action<IReadOnlyList<TError>> action);

    /// <summary>
    /// Returns the value if valid, otherwise returns the provided default value.
    /// </summary>
    public abstract T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns the value if valid, otherwise calls the provided factory function.
    /// </summary>
    public abstract T GetValueOrDefault(Func<T> defaultFactory);

    /// <summary>
    /// Returns the value if valid, otherwise throws an exception.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validation failed.</exception>
    public abstract T GetValueOrThrow();

    /// <summary>
    /// Implicitly converts a value to Valid.
    /// </summary>
    public static implicit operator Validation<T, TError>(T value) => new Valid<T, TError>(value);
}

/// <summary>
/// Represents a successful validation with a value.
/// </summary>
public sealed record Valid<T, TError>(T Value) : Validation<T, TError> where TError : Error
{
    /// <inheritdoc />
    public override bool IsValid => true;

    /// <inheritdoc />
    public override TResult Match<TResult>(Func<T, TResult> valid, Func<IReadOnlyList<TError>, TResult> invalid) =>
        valid(Value);

    /// <inheritdoc />
    public override async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> valid, Func<IReadOnlyList<TError>, Task<TResult>> invalid) =>
        await valid(Value);

    /// <inheritdoc />
    public override Validation<TResult, TError> Map<TResult>(Func<T, TResult> mapper) =>
        new Valid<TResult, TError>(mapper(Value));

    /// <inheritdoc />
    public override async Task<Validation<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        new Valid<TResult, TError>(await mapper(Value));

    /// <inheritdoc />
    public override Validation<TResult, TError> Bind<TResult>(Func<T, Validation<TResult, TError>> binder) =>
        binder(Value);

    /// <inheritdoc />
    public override async Task<Validation<TResult, TError>> BindAsync<TResult>(
        Func<T, Task<Validation<TResult, TError>>> binder) =>
        await binder(Value);

    /// <inheritdoc />
    public override Validation<T, TError> Tap(Action<T> action)
    {
        action(Value);
        return this;
    }

    /// <inheritdoc />
    public override Validation<T, TError> TapInvalid(Action<IReadOnlyList<TError>> action) => this;

    /// <inheritdoc />
    public override T GetValueOrDefault(T defaultValue) => Value;

    /// <inheritdoc />
    public override T GetValueOrDefault(Func<T> defaultFactory) => Value;

    /// <inheritdoc />
    public override T GetValueOrThrow() => Value;
}

/// <summary>
/// Represents a failed validation with one or more errors.
/// </summary>
public sealed record Invalid<T, TError>(IReadOnlyList<TError> Errors) : Validation<T, TError> where TError : Error
{
    /// <inheritdoc />
    public override bool IsValid => false;

    /// <inheritdoc />
    public override TResult Match<TResult>(Func<T, TResult> valid, Func<IReadOnlyList<TError>, TResult> invalid) =>
        invalid(Errors);

    /// <inheritdoc />
    public override async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> valid, Func<IReadOnlyList<TError>, Task<TResult>> invalid) =>
        await invalid(Errors);

    /// <inheritdoc />
    public override Validation<TResult, TError> Map<TResult>(Func<T, TResult> mapper) =>
        new Invalid<TResult, TError>(Errors);

    /// <inheritdoc />
    public override Task<Validation<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        Task.FromResult<Validation<TResult, TError>>(new Invalid<TResult, TError>(Errors));

    /// <inheritdoc />
    public override Validation<TResult, TError> Bind<TResult>(Func<T, Validation<TResult, TError>> binder) =>
        new Invalid<TResult, TError>(Errors);

    /// <inheritdoc />
    public override Task<Validation<TResult, TError>> BindAsync<TResult>(
        Func<T, Task<Validation<TResult, TError>>> binder) =>
        Task.FromResult<Validation<TResult, TError>>(new Invalid<TResult, TError>(Errors));

    /// <inheritdoc />
    public override Validation<T, TError> Tap(Action<T> action) => this;

    /// <inheritdoc />
    public override Validation<T, TError> TapInvalid(Action<IReadOnlyList<TError>> action)
    {
        action(Errors);
        return this;
    }

    /// <inheritdoc />
    public override T GetValueOrDefault(T defaultValue) => defaultValue;

    /// <inheritdoc />
    public override T GetValueOrDefault(Func<T> defaultFactory) => defaultFactory();

    /// <inheritdoc />
    public override T GetValueOrThrow() =>
        throw new InvalidOperationException(
            $"Cannot get value from an invalid validation. Errors: {string.Join(", ", Errors.Select(e => e.Message))}");
}

/// <summary>
/// Provides static factory methods for creating Validation instances.
/// </summary>
public static class Validation
{
    /// <summary>
    /// Creates a successful validation with a value.
    /// </summary>
    public static Validation<T, TError> Valid<T, TError>(T value) where TError : Error =>
        new Valid<T, TError>(value);

    /// <summary>
    /// Creates a failed validation with a single error.
    /// </summary>
    public static Validation<T, TError> Invalid<T, TError>(TError error) where TError : Error =>
        new Invalid<T, TError>([error]);

    /// <summary>
    /// Creates a failed validation with multiple errors.
    /// </summary>
    public static Validation<T, TError> Invalid<T, TError>(IEnumerable<TError> errors) where TError : Error =>
        new Invalid<T, TError>(errors.ToList().AsReadOnly());
}
