namespace DarkPeak.Functional;

/// <summary>
/// Represents the result of a validation that can either succeed with a value
/// or fail with one or more errors. Unlike <see cref="Result{T, TError}"/>, multiple
/// <see cref="Validation{T, TError}"/> values can be combined via <c>Apply</c> to accumulate all errors.
/// </summary>
/// <typeparam name="T">The type of the validated value.</typeparam>
/// <typeparam name="TError">The type of the error, must inherit from <see cref="Error"/>.</typeparam>
public union Validation<T, TError>(Valid<T, TError>, Invalid<T, TError>) where TError : Error;

/// <summary>
/// Represents a successful validation with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public record Valid<T, TError>(T Value) where TError : Error;

/// <summary>
/// Represents a failed validation with one or more errors.
/// </summary>
/// <typeparam name="T">The type parameter.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public record Invalid<T, TError>(IReadOnlyList<TError> Errors) where TError : Error;

/// <summary>
/// Provides static factory methods and extension methods for <see cref="Validation{T, TError}"/>.
/// </summary>
public static class Validation
{
    /// <summary>Creates a successful validation with a value.</summary>
    public static Validation<T, TError> Valid<T, TError>(T value) where TError : Error =>
        new Valid<T, TError>(value);

    /// <summary>Creates a failed validation with a single error.</summary>
    public static Validation<T, TError> Invalid<T, TError>(TError error) where TError : Error =>
        new Invalid<T, TError>([error]);

    /// <summary>Creates a failed validation with multiple errors.</summary>
    public static Validation<T, TError> Invalid<T, TError>(IEnumerable<TError> errors) where TError : Error =>
        new Invalid<T, TError>(errors.ToList().AsReadOnly());

    // ── State checks ──

    /// <summary>Gets a value indicating whether the validation succeeded.</summary>
    public static bool IsValid<T, TError>(this Validation<T, TError> validation) where TError : Error =>
        validation is Valid<T, TError>;

    /// <summary>Gets a value indicating whether the validation failed.</summary>
    public static bool IsInvalid<T, TError>(this Validation<T, TError> validation) where TError : Error =>
        validation is Invalid<T, TError>;

    // ── Match ──

    /// <summary>
    /// Matches the validation to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="validation">The validation to match.</param>
    /// <param name="valid">Function to call if the validation succeeded.</param>
    /// <param name="invalid">Function to call if the validation failed.</param>
    /// <returns>The result of the matching function.</returns>
    public static TResult Match<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, TResult> valid,
        Func<IReadOnlyList<TError>, TResult> invalid)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }     => valid(v),
            Invalid<T, TError> { Errors: var e }  => invalid(e)
        };

    /// <summary>
    /// Asynchronously matches the validation to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="validation">The validation to match.</param>
    /// <param name="valid">Async function to call if the validation succeeded.</param>
    /// <param name="invalid">Async function to call if the validation failed.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public static Task<TResult> MatchAsync<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, Task<TResult>> valid,
        Func<IReadOnlyList<TError>, Task<TResult>> invalid)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => valid(v),
            Invalid<T, TError> { Errors: var e } => invalid(e)
        };

    // ── Map ──

    /// <summary>
    /// Transforms the value inside the validation using the provided function.
    /// </summary>
    /// <typeparam name="T">The source validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="validation">The validation to map.</param>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>A validation containing the transformed value, or the original errors.</returns>
    public static Validation<TResult, TError> Map<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, TResult> mapper)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => new Valid<TResult, TError>(mapper(v)),
            Invalid<T, TError> { Errors: var e } => new Invalid<TResult, TError>(e)
        };

    /// <summary>
    /// Asynchronously transforms the value inside the validation using the provided function.
    /// </summary>
    /// <typeparam name="T">The source validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="validation">The validation to map.</param>
    /// <param name="mapper">Async function to transform the value.</param>
    /// <returns>A task containing a validation with the transformed value, or the original errors.</returns>
    public static async Task<Validation<TResult, TError>> MapAsync<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, Task<TResult>> mapper)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => new Valid<TResult, TError>(await mapper(v)),
            Invalid<T, TError> { Errors: var e } => new Invalid<TResult, TError>(e)
        };

    // ── Bind ──

    /// <summary>
    /// Binds the validation to a function that returns another validation (short-circuits on failure).
    /// Use <c>Apply</c> instead if you want to accumulate errors.
    /// </summary>
    /// <typeparam name="T">The source validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="validation">The validation to bind.</param>
    /// <param name="binder">Function that returns a validation.</param>
    /// <returns>The result of the binder function, or the original errors.</returns>
    public static Validation<TResult, TError> Bind<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, Validation<TResult, TError>> binder)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => binder(v),
            Invalid<T, TError> { Errors: var e } => new Invalid<TResult, TError>(e)
        };

    /// <summary>
    /// Asynchronously binds the validation to a function that returns another validation (short-circuits on failure).
    /// </summary>
    /// <typeparam name="T">The source validated value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="validation">The validation to bind.</param>
    /// <param name="binder">Async function that returns a validation.</param>
    /// <returns>A task containing the result of the binder function, or the original errors.</returns>
    public static Task<Validation<TResult, TError>> BindAsync<T, TError, TResult>(
        this Validation<T, TError> validation,
        Func<T, Task<Validation<TResult, TError>>> binder)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => binder(v),
            Invalid<T, TError> { Errors: var e } => Task.FromResult<Validation<TResult, TError>>(new Invalid<TResult, TError>(e))
        };

    // ── Tap ──

    /// <summary>
    /// Executes an action if the validation succeeded (side effect).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <param name="action">Action to execute with the value.</param>
    /// <returns>This validation for chaining.</returns>
    public static Validation<T, TError> Tap<T, TError>(this Validation<T, TError> validation, Action<T> action)
        where TError : Error
    {
        if (validation is Valid<T, TError> { Value: var v }) action(v);
        return validation;
    }

    /// <summary>
    /// Executes an action if the validation failed (side effect).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <param name="action">Action to execute with the errors.</param>
    /// <returns>This validation for chaining.</returns>
    public static Validation<T, TError> TapInvalid<T, TError>(
        this Validation<T, TError> validation,
        Action<IReadOnlyList<TError>> action)
        where TError : Error
    {
        if (validation is Invalid<T, TError> { Errors: var e }) action(e);
        return validation;
    }

    // ── Get value ──

    /// <summary>
    /// Returns the value if valid, otherwise returns the provided default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <param name="defaultValue">The default value to return if the validation failed.</param>
    /// <returns>The validation's value or the default value.</returns>
    public static T GetValueOrDefault<T, TError>(this Validation<T, TError> validation, T defaultValue)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v } => v,
            Invalid<T, TError>               => defaultValue
        };

    /// <summary>
    /// Returns the value if valid, otherwise calls the provided factory function.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <param name="defaultFactory">Function to produce a default value.</param>
    /// <returns>The validation's value or the result of the factory function.</returns>
    public static T GetValueOrDefault<T, TError>(this Validation<T, TError> validation, Func<T> defaultFactory)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v } => v,
            Invalid<T, TError>               => defaultFactory()
        };

    /// <summary>
    /// Returns the value if valid, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <returns>The validation's value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the validation failed.</exception>
    public static T GetValueOrThrow<T, TError>(this Validation<T, TError> validation)
        where TError : Error =>
        validation switch
        {
            Valid<T, TError> { Value: var v }    => v,
            Invalid<T, TError> { Errors: var e } => throw new InvalidOperationException(
                $"Cannot get value from an invalid validation. Errors: {string.Join(", ", e.Select(err => err.Message))}")
        };
}
