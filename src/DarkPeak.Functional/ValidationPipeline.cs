using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional;

/// <summary>
/// Fluent pipeline builder for constructing reusable multi-step Validation-based workflows.
/// Unlike <see cref="Pipeline"/>, which short-circuits on the first error, ValidationPipeline
/// runs all validation steps against the same input and <b>accumulates all errors</b>.
/// </summary>
/// <remarks>
/// <para>
/// Each <c>Validate</c> step receives the original input and produces an independent
/// <see cref="Validation{T,TError}"/>. When the pipeline is built, all steps run (fan-out),
/// errors are collected from every failing step, and the combiner function is applied only
/// when every step succeeds.
/// </para>
/// <para>
/// Async steps (<c>ValidateAsync</c>) cause the built pipeline to execute all steps
/// concurrently via <c>Task.WhenAll</c> and return a <c>Task&lt;Validation&gt;</c>.
/// </para>
/// </remarks>
public static class ValidationPipeline
{
    /// <summary>
    /// Creates a new validation pipeline builder that validates values of type <typeparamref name="TInput"/>.
    /// </summary>
    /// <typeparam name="TInput">The input type that all validation steps receive.</typeparam>
    /// <typeparam name="TError">The error type for all steps, must inherit from <see cref="Error"/>.</typeparam>
    /// <returns>A pipeline builder ready to accept validation steps.</returns>
    public static IValidationPipelineStart<TInput, TError> Create<TInput, TError>()
        where TError : Error =>
        new ValidationPipelineStart<TInput, TError>();

    // ──────────────────────────────────────────────────────────────
    //  Start interface (no steps added yet)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The initial builder state before any validation steps have been added.
    /// </summary>
    public interface IValidationPipelineStart<TInput, TError> where TError : Error
    {
        /// <summary>
        /// Adds a validation step that returns <see cref="Validation{T,TError}"/>.
        /// </summary>
        IValidationPipeline1<TInput, T1, TError> Validate<T1>(Func<TInput, Validation<T1, TError>> step);

        /// <summary>
        /// Adds a plain mapping step (auto-wrapped in <see cref="Valid{T,TError}"/>).
        /// </summary>
        IValidationPipeline1<TInput, T1, TError> Validate<T1>(Func<TInput, T1> step);

        /// <summary>
        /// Adds an async validation step, transitioning the pipeline to async mode.
        /// </summary>
        IAsyncValidationPipeline1<TInput, T1, TError> ValidateAsync<T1>(Func<TInput, Task<Validation<T1, TError>>> step);
    }

    // ──────────────────────────────────────────────────────────────
    //  Sync pipeline interfaces (arity 1–8)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Validation pipeline builder with 1 step.
    /// </summary>
    public interface IValidationPipeline1<TInput, T1, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, Validation<T2, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, T2> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline2<TInput, T1, T2, TError> ValidateAsync<T2>(Func<TInput, Task<Validation<T2, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<T1, TError>> Build();
    }

    /// <summary>
    /// Validation pipeline builder with 2 steps.
    /// </summary>
    public interface IValidationPipeline2<TInput, T1, T2, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, Validation<T3, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, T3> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> ValidateAsync<T3>(Func<TInput, Task<Validation<T3, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 3 steps.
    /// </summary>
    public interface IValidationPipeline3<TInput, T1, T2, T3, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, Validation<T4, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, T4> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> ValidateAsync<T4>(Func<TInput, Task<Validation<T4, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 4 steps.
    /// </summary>
    public interface IValidationPipeline4<TInput, T1, T2, T3, T4, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, Validation<T5, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, T5> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> ValidateAsync<T5>(Func<TInput, Task<Validation<T5, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 5 steps.
    /// </summary>
    public interface IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, Validation<T6, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, T6> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> ValidateAsync<T6>(Func<TInput, Task<Validation<T6, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 6 steps.
    /// </summary>
    public interface IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, Validation<T7, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, T7> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> ValidateAsync<T7>(Func<TInput, Task<Validation<T7, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 7 steps.
    /// </summary>
    public interface IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, Validation<T8, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, T8> step);

        /// <summary>Adds an async validation step, transitioning the pipeline to async mode.</summary>
        IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> ValidateAsync<T8>(Func<TInput, Task<Validation<T8, TError>>> step);

        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> combiner);
    }

    /// <summary>
    /// Validation pipeline builder with 8 steps (maximum arity).
    /// </summary>
    public interface IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> where TError : Error
    {
        /// <summary>Builds the pipeline into a reusable function.</summary>
        Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> combiner);
    }

    // ──────────────────────────────────────────────────────────────
    //  Async pipeline interfaces (arity 1–8)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Async validation pipeline builder with 1 step.
    /// </summary>
    public interface IAsyncValidationPipeline1<TInput, T1, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, Validation<T2, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, T2> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline2<TInput, T1, T2, TError> ValidateAsync<T2>(Func<TInput, Task<Validation<T2, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<T1, TError>>> Build();
    }

    /// <summary>
    /// Async validation pipeline builder with 2 steps.
    /// </summary>
    public interface IAsyncValidationPipeline2<TInput, T1, T2, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, Validation<T3, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, T3> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> ValidateAsync<T3>(Func<TInput, Task<Validation<T3, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 3 steps.
    /// </summary>
    public interface IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, Validation<T4, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, T4> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> ValidateAsync<T4>(Func<TInput, Task<Validation<T4, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 4 steps.
    /// </summary>
    public interface IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, Validation<T5, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, T5> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> ValidateAsync<T5>(Func<TInput, Task<Validation<T5, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 5 steps.
    /// </summary>
    public interface IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, Validation<T6, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, T6> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> ValidateAsync<T6>(Func<TInput, Task<Validation<T6, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 6 steps.
    /// </summary>
    public interface IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, Validation<T7, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, T7> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> ValidateAsync<T7>(Func<TInput, Task<Validation<T7, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 7 steps.
    /// </summary>
    public interface IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> where TError : Error
    {
        /// <summary>Adds a validation step.</summary>
        IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, Validation<T8, TError>> step);

        /// <summary>Adds a plain mapping step (auto-wrapped in Valid).</summary>
        IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, T8> step);

        /// <summary>Adds an async validation step.</summary>
        IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> ValidateAsync<T8>(Func<TInput, Task<Validation<T8, TError>>> step);

        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> combiner);
    }

    /// <summary>
    /// Async validation pipeline builder with 8 steps (maximum arity).
    /// </summary>
    public interface IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> where TError : Error
    {
        /// <summary>Builds the pipeline into a reusable async function.</summary>
        Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> combiner);
    }

    // ──────────────────────────────────────────────────────────────
    //  Start implementation
    // ──────────────────────────────────────────────────────────────

    private sealed class ValidationPipelineStart<TInput, TError> : IValidationPipelineStart<TInput, TError>
        where TError : Error
    {
        public IValidationPipeline1<TInput, T1, TError> Validate<T1>(Func<TInput, Validation<T1, TError>> step) =>
            new SyncPipeline1<TInput, T1, TError>(step);

        public IValidationPipeline1<TInput, T1, TError> Validate<T1>(Func<TInput, T1> step) =>
            new SyncPipeline1<TInput, T1, TError>(input => Functional.Validation.Valid<T1, TError>(step(input)));

        public IAsyncValidationPipeline1<TInput, T1, TError> ValidateAsync<T1>(Func<TInput, Task<Validation<T1, TError>>> step) =>
            new AsyncPipeline1<TInput, T1, TError>(step);
    }

    // ──────────────────────────────────────────────────────────────
    //  Sync implementations (arity 1–8)
    // ──────────────────────────────────────────────────────────────

    private sealed class SyncPipeline1<TInput, T1, TError>(
        Func<TInput, Validation<T1, TError>> step1)
        : IValidationPipeline1<TInput, T1, TError>
        where TError : Error
    {
        public IValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, Validation<T2, TError>> step) =>
            new SyncPipeline2<TInput, T1, T2, TError>(step1, step);

        public IValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, T2> step) =>
            new SyncPipeline2<TInput, T1, T2, TError>(step1, input => Functional.Validation.Valid<T2, TError>(step(input)));

        public IAsyncValidationPipeline2<TInput, T1, T2, TError> ValidateAsync<T2>(Func<TInput, Task<Validation<T2, TError>>> step) =>
            new AsyncPipeline2<TInput, T1, T2, TError>(
                input => Task.FromResult(step1(input)), step);

        public Func<TInput, Validation<T1, TError>> Build() => step1;
    }

    private sealed class SyncPipeline2<TInput, T1, T2, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2)
        : IValidationPipeline2<TInput, T1, T2, TError>
        where TError : Error
    {
        public IValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, Validation<T3, TError>> step) =>
            new SyncPipeline3<TInput, T1, T2, T3, TError>(step1, step2, step);

        public IValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, T3> step) =>
            new SyncPipeline3<TInput, T1, T2, T3, TError>(step1, step2, input => Functional.Validation.Valid<T3, TError>(step(input)));

        public IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> ValidateAsync<T3>(Func<TInput, Task<Validation<T3, TError>>> step) =>
            new AsyncPipeline3<TInput, T1, T2, T3, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), combiner);
    }

    private sealed class SyncPipeline3<TInput, T1, T2, T3, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3)
        : IValidationPipeline3<TInput, T1, T2, T3, TError>
        where TError : Error
    {
        public IValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, Validation<T4, TError>> step) =>
            new SyncPipeline4<TInput, T1, T2, T3, T4, TError>(step1, step2, step3, step);

        public IValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, T4> step) =>
            new SyncPipeline4<TInput, T1, T2, T3, T4, TError>(step1, step2, step3, input => Functional.Validation.Valid<T4, TError>(step(input)));

        public IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> ValidateAsync<T4>(Func<TInput, Task<Validation<T4, TError>>> step) =>
            new AsyncPipeline4<TInput, T1, T2, T3, T4, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                input => Task.FromResult(step3(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), combiner);
    }

    private sealed class SyncPipeline4<TInput, T1, T2, T3, T4, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3,
        Func<TInput, Validation<T4, TError>> step4)
        : IValidationPipeline4<TInput, T1, T2, T3, T4, TError>
        where TError : Error
    {
        public IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, Validation<T5, TError>> step) =>
            new SyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(step1, step2, step3, step4, step);

        public IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, T5> step) =>
            new SyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(step1, step2, step3, step4, input => Functional.Validation.Valid<T5, TError>(step(input)));

        public IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> ValidateAsync<T5>(Func<TInput, Task<Validation<T5, TError>>> step) =>
            new AsyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                input => Task.FromResult(step3(input)),
                input => Task.FromResult(step4(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), step4(input), combiner);
    }

    private sealed class SyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3,
        Func<TInput, Validation<T4, TError>> step4,
        Func<TInput, Validation<T5, TError>> step5)
        : IValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError>
        where TError : Error
    {
        public IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, Validation<T6, TError>> step) =>
            new SyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(step1, step2, step3, step4, step5, step);

        public IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, T6> step) =>
            new SyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(step1, step2, step3, step4, step5, input => Functional.Validation.Valid<T6, TError>(step(input)));

        public IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> ValidateAsync<T6>(Func<TInput, Task<Validation<T6, TError>>> step) =>
            new AsyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                input => Task.FromResult(step3(input)),
                input => Task.FromResult(step4(input)),
                input => Task.FromResult(step5(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), step4(input), step5(input), combiner);
    }

    private sealed class SyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3,
        Func<TInput, Validation<T4, TError>> step4,
        Func<TInput, Validation<T5, TError>> step5,
        Func<TInput, Validation<T6, TError>> step6)
        : IValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>
        where TError : Error
    {
        public IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, Validation<T7, TError>> step) =>
            new SyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(step1, step2, step3, step4, step5, step6, step);

        public IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, T7> step) =>
            new SyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(step1, step2, step3, step4, step5, step6, input => Functional.Validation.Valid<T7, TError>(step(input)));

        public IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> ValidateAsync<T7>(Func<TInput, Task<Validation<T7, TError>>> step) =>
            new AsyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                input => Task.FromResult(step3(input)),
                input => Task.FromResult(step4(input)),
                input => Task.FromResult(step5(input)),
                input => Task.FromResult(step6(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), step4(input), step5(input), step6(input), combiner);
    }

    private sealed class SyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3,
        Func<TInput, Validation<T4, TError>> step4,
        Func<TInput, Validation<T5, TError>> step5,
        Func<TInput, Validation<T6, TError>> step6,
        Func<TInput, Validation<T7, TError>> step7)
        : IValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>
        where TError : Error
    {
        public IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, Validation<T8, TError>> step) =>
            new SyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(step1, step2, step3, step4, step5, step6, step7, step);

        public IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, T8> step) =>
            new SyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(step1, step2, step3, step4, step5, step6, step7, input => Functional.Validation.Valid<T8, TError>(step(input)));

        public IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> ValidateAsync<T8>(Func<TInput, Task<Validation<T8, TError>>> step) =>
            new AsyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(
                input => Task.FromResult(step1(input)),
                input => Task.FromResult(step2(input)),
                input => Task.FromResult(step3(input)),
                input => Task.FromResult(step4(input)),
                input => Task.FromResult(step5(input)),
                input => Task.FromResult(step6(input)),
                input => Task.FromResult(step7(input)),
                step);

        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), step4(input), step5(input), step6(input), step7(input), combiner);
    }

    private sealed class SyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        Func<TInput, Validation<T1, TError>> step1,
        Func<TInput, Validation<T2, TError>> step2,
        Func<TInput, Validation<T3, TError>> step3,
        Func<TInput, Validation<T4, TError>> step4,
        Func<TInput, Validation<T5, TError>> step5,
        Func<TInput, Validation<T6, TError>> step6,
        Func<TInput, Validation<T7, TError>> step7,
        Func<TInput, Validation<T8, TError>> step8)
        : IValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>
        where TError : Error
    {
        public Func<TInput, Validation<TResult, TError>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> combiner) =>
            input => step1(input).ZipWith(step2(input), step3(input), step4(input), step5(input), step6(input), step7(input), step8(input), combiner);
    }

    // ──────────────────────────────────────────────────────────────
    //  Async implementations (arity 1–8)
    //  All steps are stored as async funcs and executed concurrently
    //  via Task.WhenAll.
    // ──────────────────────────────────────────────────────────────

    private sealed class AsyncPipeline1<TInput, T1, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1)
        : IAsyncValidationPipeline1<TInput, T1, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, Validation<T2, TError>> step) =>
            new AsyncPipeline2<TInput, T1, T2, TError>(step1, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline2<TInput, T1, T2, TError> Validate<T2>(Func<TInput, T2> step) =>
            new AsyncPipeline2<TInput, T1, T2, TError>(step1,
                input => Task.FromResult(Functional.Validation.Valid<T2, TError>(step(input))));

        public IAsyncValidationPipeline2<TInput, T1, T2, TError> ValidateAsync<T2>(Func<TInput, Task<Validation<T2, TError>>> step) =>
            new AsyncPipeline2<TInput, T1, T2, TError>(step1, step);

        public Func<TInput, Task<Validation<T1, TError>>> Build() => step1;
    }

    private sealed class AsyncPipeline2<TInput, T1, T2, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2)
        : IAsyncValidationPipeline2<TInput, T1, T2, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, Validation<T3, TError>> step) =>
            new AsyncPipeline3<TInput, T1, T2, T3, TError>(step1, step2, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> Validate<T3>(Func<TInput, T3> step) =>
            new AsyncPipeline3<TInput, T1, T2, T3, TError>(step1, step2,
                input => Task.FromResult(Functional.Validation.Valid<T3, TError>(step(input))));

        public IAsyncValidationPipeline3<TInput, T1, T2, T3, TError> ValidateAsync<T3>(Func<TInput, Task<Validation<T3, TError>>> step) =>
            new AsyncPipeline3<TInput, T1, T2, T3, TError>(step1, step2, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), combiner);
    }

    private sealed class AsyncPipeline3<TInput, T1, T2, T3, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3)
        : IAsyncValidationPipeline3<TInput, T1, T2, T3, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, Validation<T4, TError>> step) =>
            new AsyncPipeline4<TInput, T1, T2, T3, T4, TError>(step1, step2, step3, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> Validate<T4>(Func<TInput, T4> step) =>
            new AsyncPipeline4<TInput, T1, T2, T3, T4, TError>(step1, step2, step3,
                input => Task.FromResult(Functional.Validation.Valid<T4, TError>(step(input))));

        public IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError> ValidateAsync<T4>(Func<TInput, Task<Validation<T4, TError>>> step) =>
            new AsyncPipeline4<TInput, T1, T2, T3, T4, TError>(step1, step2, step3, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), combiner);
    }

    private sealed class AsyncPipeline4<TInput, T1, T2, T3, T4, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3,
        Func<TInput, Task<Validation<T4, TError>>> step4)
        : IAsyncValidationPipeline4<TInput, T1, T2, T3, T4, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, Validation<T5, TError>> step) =>
            new AsyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(step1, step2, step3, step4, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> Validate<T5>(Func<TInput, T5> step) =>
            new AsyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(step1, step2, step3, step4,
                input => Task.FromResult(Functional.Validation.Valid<T5, TError>(step(input))));

        public IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError> ValidateAsync<T5>(Func<TInput, Task<Validation<T5, TError>>> step) =>
            new AsyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(step1, step2, step3, step4, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), step4(input), combiner);
    }

    private sealed class AsyncPipeline5<TInput, T1, T2, T3, T4, T5, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3,
        Func<TInput, Task<Validation<T4, TError>>> step4,
        Func<TInput, Task<Validation<T5, TError>>> step5)
        : IAsyncValidationPipeline5<TInput, T1, T2, T3, T4, T5, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, Validation<T6, TError>> step) =>
            new AsyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(step1, step2, step3, step4, step5, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> Validate<T6>(Func<TInput, T6> step) =>
            new AsyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(step1, step2, step3, step4, step5,
                input => Task.FromResult(Functional.Validation.Valid<T6, TError>(step(input))));

        public IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError> ValidateAsync<T6>(Func<TInput, Task<Validation<T6, TError>>> step) =>
            new AsyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(step1, step2, step3, step4, step5, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), step4(input), step5(input), combiner);
    }

    private sealed class AsyncPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3,
        Func<TInput, Task<Validation<T4, TError>>> step4,
        Func<TInput, Task<Validation<T5, TError>>> step5,
        Func<TInput, Task<Validation<T6, TError>>> step6)
        : IAsyncValidationPipeline6<TInput, T1, T2, T3, T4, T5, T6, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, Validation<T7, TError>> step) =>
            new AsyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(step1, step2, step3, step4, step5, step6, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> Validate<T7>(Func<TInput, T7> step) =>
            new AsyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(step1, step2, step3, step4, step5, step6,
                input => Task.FromResult(Functional.Validation.Valid<T7, TError>(step(input))));

        public IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError> ValidateAsync<T7>(Func<TInput, Task<Validation<T7, TError>>> step) =>
            new AsyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(step1, step2, step3, step4, step5, step6, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), step4(input), step5(input), step6(input), combiner);
    }

    private sealed class AsyncPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3,
        Func<TInput, Task<Validation<T4, TError>>> step4,
        Func<TInput, Task<Validation<T5, TError>>> step5,
        Func<TInput, Task<Validation<T6, TError>>> step6,
        Func<TInput, Task<Validation<T7, TError>>> step7)
        : IAsyncValidationPipeline7<TInput, T1, T2, T3, T4, T5, T6, T7, TError>
        where TError : Error
    {
        public IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, Validation<T8, TError>> step) =>
            new AsyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(step1, step2, step3, step4, step5, step6, step7, input => Task.FromResult(step(input)));

        public IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> Validate<T8>(Func<TInput, T8> step) =>
            new AsyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(step1, step2, step3, step4, step5, step6, step7,
                input => Task.FromResult(Functional.Validation.Valid<T8, TError>(step(input))));

        public IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError> ValidateAsync<T8>(Func<TInput, Task<Validation<T8, TError>>> step) =>
            new AsyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(step1, step2, step3, step4, step5, step6, step7, step);

        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), step4(input), step5(input), step6(input), step7(input), combiner);
    }

    private sealed class AsyncPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        Func<TInput, Task<Validation<T1, TError>>> step1,
        Func<TInput, Task<Validation<T2, TError>>> step2,
        Func<TInput, Task<Validation<T3, TError>>> step3,
        Func<TInput, Task<Validation<T4, TError>>> step4,
        Func<TInput, Task<Validation<T5, TError>>> step5,
        Func<TInput, Task<Validation<T6, TError>>> step6,
        Func<TInput, Task<Validation<T7, TError>>> step7,
        Func<TInput, Task<Validation<T8, TError>>> step8)
        : IAsyncValidationPipeline8<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TError>
        where TError : Error
    {
        public Func<TInput, Task<Validation<TResult, TError>>> Build<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> combiner) =>
            input => step1(input).ZipWithAsync(step2(input), step3(input), step4(input), step5(input), step6(input), step7(input), step8(input), combiner);
    }
}
