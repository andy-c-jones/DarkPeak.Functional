using System.Collections;

namespace DarkPeak.Functional;

/// <summary>
/// Represents an optional value that may or may not be present.
/// Provides a type-safe alternative to null references.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public union Option<T>(Some<T>, None<T>);

/// <summary>Represents an option with a value.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
public record Some<T>(T Value);

/// <summary>Represents an empty option with no value.</summary>
/// <typeparam name="T">The type parameter.</typeparam>
public record None<T>;

/// <summary>
/// Provides static factory methods and extension methods for <see cref="Option{T}"/>.
/// </summary>
public static class Option
{
    /// <summary>Creates an option with a value.</summary>
    public static Option<T> Some<T>(T value) => new Some<T>(value);

    /// <summary>Creates an empty option.</summary>
    public static Option<T> None<T>() => new None<T>();

    /// <summary>Converts a nullable reference type to an option.</summary>
    public static Option<T> From<T>(T? value) where T : class =>
        value is not null ? new Some<T>(value) : new None<T>();

    /// <summary>Converts a nullable value type to an option.</summary>
    public static Option<T> From<T>(T? value) where T : struct =>
        value.HasValue ? new Some<T>(value.Value) : new None<T>();

    /// <summary>
    /// Executes a function and wraps the result in an option.
    /// Returns Some if the function succeeds, None if it throws an exception.
    /// </summary>
    public static Option<T> Try<T>(Func<T> func)
    {
        try { return new Some<T>(func()); }
        catch { return new None<T>(); }
    }

    /// <summary>
    /// Executes an async function and wraps the result in an option.
    /// Returns Some if the function succeeds, None if it throws an exception.
    /// </summary>
    public static async Task<Option<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try { return new Some<T>(await func()); }
        catch { return new None<T>(); }
    }

    /// <summary>
    /// Attempts to parse a string into a value using <see cref="IParsable{T}"/>.
    /// Returns Some if parsing succeeds, None otherwise.
    /// </summary>
    public static Option<T> TryParse<T>(string? value) where T : IParsable<T> =>
        T.TryParse(value, null, out var result) ? new Some<T>(result) : new None<T>();

    /// <summary>
    /// Attempts to parse a string into a value using <see cref="IParsable{T}"/> with a format provider.
    /// Returns Some if parsing succeeds, None otherwise.
    /// </summary>
    public static Option<T> TryParse<T>(string? value, IFormatProvider? provider) where T : IParsable<T> =>
        T.TryParse(value, provider, out var result) ? new Some<T>(result) : new None<T>();

    // ── State checks ──

    /// <summary>Gets a value indicating whether this option contains a value.</summary>
    public static bool IsSome<T>(this Option<T> option) => option is Some<T>;

    /// <summary>Gets a value indicating whether this option is empty.</summary>
    public static bool IsNone<T>(this Option<T> option) => option is None<T>;

    // ── Match ──

    /// <summary>
    /// Matches the option to one of two functions based on whether it has a value.
    /// </summary>
    /// <typeparam name="T">The option value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="option">The option to match.</param>
    /// <param name="some">Function to call if the option has a value.</param>
    /// <param name="none">Function to call if the option is empty.</param>
    /// <returns>The result of the matching function.</returns>
    public static TResult Match<T, TResult>(this Option<T> option, Func<T, TResult> some, Func<TResult> none) =>
        option switch
        {
            Some<T> { Value: var v } => some(v),
            None<T>                  => none()
        };

    /// <summary>
    /// Asynchronously matches the option to one of two functions based on whether it has a value.
    /// </summary>
    /// <typeparam name="T">The option value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="option">The option to match.</param>
    /// <param name="some">Async function to call if the option has a value.</param>
    /// <param name="none">Async function to call if the option is empty.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public static Task<TResult> MatchAsync<T, TResult>(this Option<T> option, Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
        option switch
        {
            Some<T> { Value: var v } => some(v),
            None<T>                  => none()
        };

    // ── Map ──

    /// <summary>
    /// Transforms the value inside the option using the provided function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="option">The option to map.</param>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>An option containing the transformed value, or None if this option is empty.</returns>
    public static Option<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> mapper) =>
        option switch
        {
            Some<T> { Value: var v } => new Some<TResult>(mapper(v)),
            None<T>                  => new None<TResult>()
        };

    /// <summary>
    /// Asynchronously transforms the value inside the option using the provided function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="option">The option to map.</param>
    /// <param name="mapper">Async function to transform the value.</param>
    /// <returns>A task containing an option with the transformed value, or None if this option is empty.</returns>
    public static async Task<Option<TResult>> MapAsync<T, TResult>(this Option<T> option, Func<T, Task<TResult>> mapper) =>
        option switch
        {
            Some<T> { Value: var v } => new Some<TResult>(await mapper(v)),
            None<T>                  => new None<TResult>()
        };

    // ── Bind ──

    /// <summary>
    /// Binds the option to a function that returns another option (flatMap).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="option">The option to bind.</param>
    /// <param name="binder">Function that returns an option.</param>
    /// <returns>The result of the binder function, or None if this option is empty.</returns>
    public static Option<TResult> Bind<T, TResult>(this Option<T> option, Func<T, Option<TResult>> binder) =>
        option switch
        {
            Some<T> { Value: var v } => binder(v),
            None<T>                  => new None<TResult>()
        };

    /// <summary>
    /// Asynchronously binds the option to a function that returns another option.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="option">The option to bind.</param>
    /// <param name="binder">Async function that returns an option.</param>
    /// <returns>A task containing the result of the binder function, or None if this option is empty.</returns>
    public static Task<Option<TResult>> BindAsync<T, TResult>(this Option<T> option, Func<T, Task<Option<TResult>>> binder) =>
        option switch
        {
            Some<T> { Value: var v } => binder(v),
            None<T>                  => Task.FromResult<Option<TResult>>(new None<TResult>())
        };

    // ── Filter ──

    /// <summary>
    /// Filters the option based on a predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">Function to test the value.</param>
    /// <returns>This option if it has a value and the predicate returns true, otherwise None.</returns>
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate) =>
        option switch
        {
            Some<T> { Value: var v } when predicate(v) => option,
            _                                          => new None<T>()
        };

    // ── Get value ──

    /// <summary>
    /// Returns the value if present, otherwise returns the provided default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="defaultValue">The default value to return if the option is empty.</param>
    /// <returns>The option's value or the default value.</returns>
    public static T GetValueOrDefault<T>(this Option<T> option, T defaultValue) =>
        option switch
        {
            Some<T> { Value: var v } => v,
            None<T>                  => defaultValue
        };

    /// <summary>
    /// Returns the value if present, otherwise calls the provided factory function.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="defaultFactory">Function to produce a default value.</param>
    /// <returns>The option's value or the result of the factory function.</returns>
    public static T GetValueOrDefault<T>(this Option<T> option, Func<T> defaultFactory) =>
        option switch
        {
            Some<T> { Value: var v } => v,
            None<T>                  => defaultFactory()
        };

    /// <summary>
    /// Returns the value if present, otherwise throws an exception.
    /// Use this as an escape hatch — prefer <see cref="Match{T, TResult}"/> or <see cref="GetValueOrDefault{T}(Option{T}, T)"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <returns>The option's value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the option is empty.</exception>
    public static T GetValueOrThrow<T>(this Option<T> option) =>
        option switch
        {
            Some<T> { Value: var v } => v,
            None<T>                  => throw new InvalidOperationException("Cannot get value from None.")
        };

    // ── OrElse ──

    /// <summary>
    /// Returns this option if it has a value, otherwise returns the alternative option.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="alternative">The alternative option to return if this is None.</param>
    /// <returns>This option or the alternative.</returns>
    public static Option<T> OrElse<T>(this Option<T> option, Option<T> alternative) =>
        option switch
        {
            Some<T> => option,
            None<T> => alternative
        };

    /// <summary>
    /// Returns this option if it has a value, otherwise calls the provided factory function.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="alternativeFactory">Function to produce an alternative option.</param>
    /// <returns>This option or the result of the factory function.</returns>
    public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> alternativeFactory) =>
        option switch
        {
            Some<T> => option,
            None<T> => alternativeFactory()
        };

    // ── Tap ──

    /// <summary>
    /// Executes an action if the option has a value (side effect).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="action">Action to execute with the value.</param>
    /// <returns>This option for chaining.</returns>
    public static Option<T> Tap<T>(this Option<T> option, Action<T> action)
    {
        if (option is Some<T> { Value: var v }) action(v);
        return option;
    }

    /// <summary>
    /// Executes an action if the option is empty (side effect).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>This option for chaining.</returns>
    public static Option<T> TapNone<T>(this Option<T> option, Action action)
    {
        if (option is None<T>) action();
        return option;
    }

    // ── IEnumerable support ──

    /// <summary>
    /// Returns an enumerable that yields the value if present, or is empty if None.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option.</param>
    /// <returns>A sequence of zero or one element.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this Option<T> option)
    {
        if (option is Some<T> { Value: var v }) yield return v;
    }

    // ── LINQ support ──

    /// <summary>Maps the option using a selector (for LINQ support).</summary>
    public static Option<TResult> Select<T, TResult>(this Option<T> option, Func<T, TResult> selector) =>
        option.Map(selector);

    /// <summary>Binds the option using a selector (for LINQ query syntax support).</summary>
    public static Option<TResult> SelectMany<T, TResult>(this Option<T> option, Func<T, Option<TResult>> selector) =>
        option.Bind(selector);

    /// <summary>Binds and projects the option (for LINQ query syntax support).</summary>
    public static Option<TResult> SelectMany<T, TIntermediate, TResult>(
        this Option<T> option,
        Func<T, Option<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> projector) =>
        option.Bind(value => selector(value).Map(intermediate => projector(value, intermediate)));

    /// <summary>Filters the option using a predicate (for LINQ support).</summary>
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Filter(predicate);
}
