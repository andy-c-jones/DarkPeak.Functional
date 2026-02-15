namespace DarkPeak.Functional;

/// <summary>
/// Extension methods for piping values through functions (F#-style |> operator).
/// </summary>
public static class PipeExtensions
{
    /// <summary>
    /// Pipes a value into a function. Equivalent to <c>func(value)</c>.
    /// Enables left-to-right reading of data transformations.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TResult">The output type.</typeparam>
    /// <param name="value">The value to pipe.</param>
    /// <param name="func">The function to apply.</param>
    /// <returns>The result of applying the function to the value.</returns>
    public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> func) =>
        func(value);

    /// <summary>
    /// Pipes a value into an async function.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TResult">The output type.</typeparam>
    /// <param name="value">The value to pipe.</param>
    /// <param name="func">The async function to apply.</param>
    /// <returns>A task containing the result of applying the function to the value.</returns>
    public static Task<TResult> PipeAsync<T, TResult>(this T value, Func<T, Task<TResult>> func) =>
        func(value);
}

/// <summary>
/// Extension methods for composing functions (forward composition).
/// </summary>
public static class ComposeExtensions
{
    /// <summary>
    /// Composes two functions in forward order: <c>x => g(f(x))</c>.
    /// </summary>
    /// <typeparam name="A">The input type.</typeparam>
    /// <typeparam name="B">The intermediate type.</typeparam>
    /// <typeparam name="C">The output type.</typeparam>
    /// <param name="f">The first function.</param>
    /// <param name="g">The second function.</param>
    /// <returns>A composed function that applies f then g.</returns>
    public static Func<A, C> Compose<A, B, C>(this Func<A, B> f, Func<B, C> g) =>
        x => g(f(x));

    /// <summary>
    /// Composes a synchronous function with an async function.
    /// </summary>
    /// <typeparam name="A">The input type.</typeparam>
    /// <typeparam name="B">The intermediate type.</typeparam>
    /// <typeparam name="C">The output type.</typeparam>
    /// <param name="f">The first (synchronous) function.</param>
    /// <param name="g">The second (async) function.</param>
    /// <returns>A composed async function that applies f then g.</returns>
    public static Func<A, Task<C>> ComposeAsync<A, B, C>(this Func<A, B> f, Func<B, Task<C>> g) =>
        x => g(f(x));

    /// <summary>
    /// Composes two Result-returning functions. The composed function short-circuits on failure.
    /// </summary>
    /// <typeparam name="A">The input type.</typeparam>
    /// <typeparam name="B">The intermediate success type.</typeparam>
    /// <typeparam name="C">The output success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="f">The first Result-returning function.</param>
    /// <param name="g">The second Result-returning function.</param>
    /// <returns>A composed function that applies f then binds g.</returns>
    public static Func<A, Result<C, TError>> Compose<A, B, C, TError>(
        this Func<A, Result<B, TError>> f, Func<B, Result<C, TError>> g)
        where TError : Error =>
        x => f(x).Bind(g);

    /// <summary>
    /// Composes two async Result-returning functions. The composed function short-circuits on failure.
    /// </summary>
    /// <typeparam name="A">The input type.</typeparam>
    /// <typeparam name="B">The intermediate success type.</typeparam>
    /// <typeparam name="C">The output success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="f">The first async Result-returning function.</param>
    /// <param name="g">The second async Result-returning function.</param>
    /// <returns>A composed async function that applies f then binds g.</returns>
    public static Func<A, Task<Result<C, TError>>> ComposeAsync<A, B, C, TError>(
        this Func<A, Task<Result<B, TError>>> f, Func<B, Task<Result<C, TError>>> g)
        where TError : Error =>
        async x => await (await f(x)).BindAsync(g);

    /// <summary>
    /// Composes two Option-returning functions. The composed function short-circuits on None.
    /// </summary>
    /// <typeparam name="A">The input type.</typeparam>
    /// <typeparam name="B">The intermediate value type.</typeparam>
    /// <typeparam name="C">The output value type.</typeparam>
    /// <param name="f">The first Option-returning function.</param>
    /// <param name="g">The second Option-returning function.</param>
    /// <returns>A composed function that applies f then binds g.</returns>
    public static Func<A, Option<C>> Compose<A, B, C>(
        this Func<A, Option<B>> f, Func<B, Option<C>> g) =>
        x => f(x).Bind(g);
}

/// <summary>
/// Fluent pipeline builder for constructing reusable multi-step Result-based workflows.
/// </summary>
public static class Pipeline
{
    /// <summary>
    /// Creates a new pipeline builder that starts with a value of type <typeparamref name="TInput"/>.
    /// The pipeline produces <see cref="Result{T, TError}"/> at each step.
    /// </summary>
    /// <typeparam name="TInput">The input type for the pipeline.</typeparam>
    /// <typeparam name="TError">The error type for all steps.</typeparam>
    /// <returns>A pipeline builder ready to accept steps.</returns>
    public static IPipelineBuilder<TInput, TInput, TError> Create<TInput, TError>()
        where TError : Error =>
        new SyncPipelineBuilder<TInput, TInput, TError>(
            input => Result.Success<TInput, TError>(input));

    /// <summary>
    /// Defines the synchronous pipeline builder interface.
    /// </summary>
    /// <typeparam name="TInput">The original input type.</typeparam>
    /// <typeparam name="TCurrent">The current output type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    public interface IPipelineBuilder<TInput, TCurrent, TError> where TError : Error
    {
        /// <summary>
        /// Adds a Result-returning step to the pipeline.
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">A function that takes the current value and returns a Result.</param>
        /// <returns>A new pipeline builder with the added step.</returns>
        IPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, Result<TNext, TError>> step);

        /// <summary>
        /// Adds a plain mapping step to the pipeline (auto-wrapped in Success).
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">A function that transforms the current value.</param>
        /// <returns>A new pipeline builder with the added step.</returns>
        IPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, TNext> step);

        /// <summary>
        /// Adds an async Result-returning step, transitioning the pipeline to async mode.
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">An async function that takes the current value and returns a Result.</param>
        /// <returns>An async pipeline builder with the added step.</returns>
        IAsyncPipelineBuilder<TInput, TNext, TError> ThenAsync<TNext>(Func<TCurrent, Task<Result<TNext, TError>>> step);

        /// <summary>
        /// Builds the pipeline into a reusable function.
        /// </summary>
        /// <returns>A function that executes the full pipeline.</returns>
        Func<TInput, Result<TCurrent, TError>> Build();
    }

    /// <summary>
    /// Defines the async pipeline builder interface.
    /// </summary>
    /// <typeparam name="TInput">The original input type.</typeparam>
    /// <typeparam name="TCurrent">The current output type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    public interface IAsyncPipelineBuilder<TInput, TCurrent, TError> where TError : Error
    {
        /// <summary>
        /// Adds a Result-returning step to the async pipeline.
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">A function that takes the current value and returns a Result.</param>
        /// <returns>A new async pipeline builder with the added step.</returns>
        IAsyncPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, Result<TNext, TError>> step);

        /// <summary>
        /// Adds a plain mapping step to the async pipeline (auto-wrapped in Success).
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">A function that transforms the current value.</param>
        /// <returns>A new async pipeline builder with the added step.</returns>
        IAsyncPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, TNext> step);

        /// <summary>
        /// Adds an async Result-returning step to the async pipeline.
        /// </summary>
        /// <typeparam name="TNext">The output type of this step.</typeparam>
        /// <param name="step">An async function that takes the current value and returns a Result.</param>
        /// <returns>A new async pipeline builder with the added step.</returns>
        IAsyncPipelineBuilder<TInput, TNext, TError> ThenAsync<TNext>(Func<TCurrent, Task<Result<TNext, TError>>> step);

        /// <summary>
        /// Builds the async pipeline into a reusable function.
        /// </summary>
        /// <returns>A function that executes the full async pipeline.</returns>
        Func<TInput, Task<Result<TCurrent, TError>>> Build();
    }

    private sealed class SyncPipelineBuilder<TInput, TCurrent, TError>(
        Func<TInput, Result<TCurrent, TError>> pipeline)
        : IPipelineBuilder<TInput, TCurrent, TError> where TError : Error
    {
        public IPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, Result<TNext, TError>> step) =>
            new SyncPipelineBuilder<TInput, TNext, TError>(input => pipeline(input).Bind(step));

        public IPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, TNext> step) =>
            new SyncPipelineBuilder<TInput, TNext, TError>(input => pipeline(input).Map(step));

        public IAsyncPipelineBuilder<TInput, TNext, TError> ThenAsync<TNext>(
            Func<TCurrent, Task<Result<TNext, TError>>> step) =>
            new AsyncPipelineBuilder<TInput, TNext, TError>(
                async input => await pipeline(input).BindAsync(step));

        public Func<TInput, Result<TCurrent, TError>> Build() => pipeline;
    }

    private sealed class AsyncPipelineBuilder<TInput, TCurrent, TError>(
        Func<TInput, Task<Result<TCurrent, TError>>> pipeline)
        : IAsyncPipelineBuilder<TInput, TCurrent, TError> where TError : Error
    {
        public IAsyncPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, Result<TNext, TError>> step) =>
            new AsyncPipelineBuilder<TInput, TNext, TError>(
                async input => (await pipeline(input)).Bind(step));

        public IAsyncPipelineBuilder<TInput, TNext, TError> Then<TNext>(Func<TCurrent, TNext> step) =>
            new AsyncPipelineBuilder<TInput, TNext, TError>(
                async input => (await pipeline(input)).Map(step));

        public IAsyncPipelineBuilder<TInput, TNext, TError> ThenAsync<TNext>(
            Func<TCurrent, Task<Result<TNext, TError>>> step) =>
            new AsyncPipelineBuilder<TInput, TNext, TError>(
                async input => await (await pipeline(input)).BindAsync(step));

        public Func<TInput, Task<Result<TCurrent, TError>>> Build() => pipeline;
    }
}
