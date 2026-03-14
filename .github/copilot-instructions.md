# Copilot Instructions for DarkPeak.Functional

## Build and Test Commands

```bash
# Build all projects
dotnet build

# Run all tests
dotnet test --solution DarkPeak.Functional.slnx

# Run tests for a single project
dotnet test --solution DarkPeak.Functional.slnx --filter "DarkPeak.Functional.Tests"

# Run a single test by name
dotnet test --solution DarkPeak.Functional.slnx -- --treenode-filter "/*/*/OptionShould/Map_transforms_some_value"

# Run all tests with code coverage (cobertura XML output)
dotnet test --solution DarkPeak.Functional.slnx -- --coverage --coverage-output-format cobertura

# Generate a human-readable coverage report (requires dotnet-reportgenerator-globaltool)
reportgenerator "-reports:tests/**/TestResults/*.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:TextSummary"
cat coverage-report/Summary.txt

# Generate an HTML coverage report for detailed exploration
reportgenerator "-reports:tests/**/TestResults/*.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html"
```

## Architecture Overview

This is a .NET 10 functional programming library providing monadic types for railway-oriented programming. The solution is composed of eight packable libraries and their corresponding test projects. The library targets nullable-enabled C# with strict null reference handling.

### Solution Structure

```
DarkPeak.Functional/
├── Directory.Build.props           # Shared build properties (TFM, nullable, NuGet metadata)
├── GitVersion.yml                  # Version calculation from conventional commits
├── src/
│   ├── DarkPeak.Functional/        # Core library: monadic types, resilience policies, and extensions
│   ├── DarkPeak.Functional.Http/   # HTTP client extensions wrapping HttpClient in Result<T, Error>
│   ├── DarkPeak.Functional.AspNet/ # ASP.NET integration (ToIResult, ProblemDetails)
│   ├── DarkPeak.Functional.HealthChecks/ # Health check integration for resilience policies
│   ├── DarkPeak.Functional.Redis/  # Redis distributed cache provider for Memoize
│   ├── DarkPeak.Functional.Dapper/ # Dapper extensions wrapping queries/executes in Result<T, Error>
│   ├── DarkPeak.Functional.EntityFramework/ # EF Core extensions wrapping DbContext operations in Result<T, Error>
│   └── DarkPeak.Functional.Mediator/ # Mediator integration for CQRS with Result<T, Error>
├── tests/
│   ├── DarkPeak.Functional.Tests/
│   ├── DarkPeak.Functional.Http.Tests/
│   ├── DarkPeak.Functional.AspNet.Tests/
│   ├── DarkPeak.Functional.HealthChecks.Tests/
│   ├── DarkPeak.Functional.Redis.Tests/
│   ├── DarkPeak.Functional.Dapper.Tests/
│   ├── DarkPeak.Functional.EntityFramework.Tests/
│   └── DarkPeak.Functional.Mediator.Tests/
└── docs/                           # DocFX API documentation
```

### Package Dependencies

- **DarkPeak.Functional** — No external dependencies. The core library.
- **DarkPeak.Functional.Http** — Depends on `DarkPeak.Functional`. Provides `HttpClientExtensions` for wrapping `HttpClient` operations in `Result<T, Error>`.
- **DarkPeak.Functional.AspNet** — Depends on `DarkPeak.Functional`. References `Microsoft.AspNetCore.App` shared framework. Provides `ToIResult()`, `ToCreatedResult()`, `ToNoContentResult()`, and `ToProblemDetails()` extensions.
- **DarkPeak.Functional.Redis** — Depends on `DarkPeak.Functional` and `StackExchange.Redis`. Provides `RedisCacheProvider<TKey, TValue>` implementing `ICacheProvider<TKey, TValue>` for distributed caching with `Memoize` and `MemoizeResult`. Supports key prefixes, configurable JSON serialization, and TTL.
- **DarkPeak.Functional.HealthChecks** — Depends on `DarkPeak.Functional` and `Microsoft.Extensions.Diagnostics.HealthChecks`. Provides `IHealthCheck` implementations for circuit breakers and cache providers, with fluent `IHealthChecksBuilder` extension methods for registration. Supports tags for liveness/readiness separation.
- **DarkPeak.Functional.Dapper** — Depends on `DarkPeak.Functional` and `Dapper`. Wraps Dapper query and execute methods in `Result<T, Error>` with typed `DatabaseError` mapping and functional transaction support via `ExecuteInTransactionAsync`.
- **DarkPeak.Functional.EntityFramework** — Depends on `DarkPeak.Functional` and `Microsoft.EntityFrameworkCore`. Wraps EF Core `DbContext` query and save operations in `Result<T, Error>` with typed error mapping (`EntityFrameworkError`, `ConcurrencyError`, `SaveChangesError`) and functional transaction support.
- **DarkPeak.Functional.Mediator** — Depends on `DarkPeak.Functional`, `Mediator.Abstractions`, and `Microsoft.Extensions.DependencyInjection.Abstractions`. Provides convenience interfaces (`IResultCommand<T>`, `IResultQuery<T>`, `IResultRequest<T>`), pipeline behaviors (`ResultValidationBehavior`, `ResultExceptionHandler`), and `ISender` extensions for using `Result<T, Error>` with the source-generated Mediator library.

All packages are versioned together and released simultaneously via the manual Release workflow.

### Core Types (DarkPeak.Functional)

- **`Option<T>`** — Represents a value that may or may not exist (alternative to null). Implementations: `Some<T>`, `None<T>`
- **`Result<T, TError>`** — Represents an operation that can succeed with a value or fail with a typed error. Implementations: `Success<T, TError>`, `Failure<T, TError>`
- **`Either<TLeft, TRight>`** — Represents a value that can be one of two types (both valid states, not success/failure). Implementations: `Left<TLeft, TRight>`, `Right<TLeft, TRight>`
- **`Validation<T, TError>`** — Accumulates multiple errors instead of short-circuiting. Implementations: `Valid<T, TError>`, `Invalid<T, TError>`
- **`OneOf<T1, T2, ...>`** — Type-safe discriminated union for 2–8 type parameters. Each case is stored internally by index and accessed via exhaustive `Match()`. Supports implicit conversion from each type parameter. Static factory: `OneOf.First<T1, T2>(value)`, `OneOf.Second<T1, T2>(value)`, etc.
- **`Error`** — Abstract base record for typed errors with subtypes: `ValidationError`, `NotFoundError`, `UnauthorizedError`, `ForbiddenError`, `ConflictError`, `ExternalServiceError`, `BadRequestError`, `InternalError`, `TimeoutError`, `BulkheadRejectedError`
- **`RetryPolicy`** — Configurable retry with backoff strategies (None, Constant, Linear, Exponential). Entry point: `Retry.WithMaxAttempts()`
- **`CircuitBreakerPolicy`** — Circuit breaker that tracks consecutive failures and transitions between Closed, Open, and HalfOpen states to prevent cascading failures. Entry point: `CircuitBreaker.WithFailureThreshold()`. Uses a shared `CircuitBreakerStateTracker` for mutable state with `Lock`-based thread safety. Exposes current state via `GetSnapshot()`.
- **`CircuitBreakerSnapshot`** — Point-in-time read-only snapshot of circuit breaker state: `State`, `ConsecutiveFailures`, `LastFailureTime`, `ResetTimeout`.
- **`CircuitBreakerOpenError`** — Error returned when the circuit is open, with a `RetryAfter` property indicating time until half-open.
- **`TimeoutPolicy`** — Wraps async operations and returns `TimeoutError` if the configured duration is exceeded. Entry point: `TimeoutPolicy.Create()`. Fluent API: `WithTimeout()`, `WithTimeoutError()`.
- **`BulkheadPolicy`** — Limits concurrent operations with optional queue to prevent resource exhaustion. Entry point: `Bulkhead.WithMaxConcurrency()`. Fluent API: `WithMaxQueueSize()`, `OnRejected()`. Rejects with `BulkheadRejectedError` when full.
- **`ResiliencePolicy<TError>`** — Composite policy chaining Timeout → Retry → CircuitBreaker → Bulkhead. Entry point: `ResiliencePolicy.Create<TError>()`. Built via `ResiliencePolicyBuilder<TError>` with `WithTimeout()`, `WithRetry()`, `WithCircuitBreaker()`, `WithBulkhead()`, then `.Build()`.
- **`Memoize`** — Function caching with TTL, LRU eviction, and pluggable distributed cache via `ICacheProvider<TKey, TValue>`
- **`MemoizeResult`** — Function caching for `Result<T, TError>`-returning functions that only caches successful results. Failed results pass through uncached so subsequent calls retry the computation. Entry point: `MemoizeResult.Func()` / `MemoizeResult.FuncAsync()`
- **`Unit`** — Valueless type representing a successful operation with no return value. Located in the core `DarkPeak.Functional` namespace.

### Dapper Extensions (DarkPeak.Functional.Dapper)

- **`DbConnectionExtensions`** — Extension methods on `DbConnection` that wrap Dapper operations in `Result<T, Error>`:
  - `QueryResultAsync<T>()` — Query multiple rows
  - `QuerySingleResultAsync<T>()` — Query exactly one row (fails on empty or multiple)
  - `QuerySingleOrDefaultResultAsync<T>()` — Query zero or one row, returns `Result<Option<T>, Error>`
  - `QueryFirstResultAsync<T>()` — Query the first row (fails on empty)
  - `QueryFirstOrDefaultResultAsync<T>()` — Query the first row or none, returns `Result<Option<T>, Error>`
  - `ExecuteResultAsync()` — Execute a command, returns `Result<int, Error>` (rows affected)
  - `ExecuteScalarResultAsync<T>()` — Execute scalar query
- **`DbConnectionTransactionExtensions`** — Functional transaction support:
  - `ExecuteInTransactionAsync<T>()` — Executes a delegate within a transaction; commits on `Success`, rolls back on `Failure` or exception. Accepts `IsolationLevel` (defaults to `ReadCommitted`). Auto-opens the connection if closed.
  - `ExecuteInTransactionAsync()` — Unit-returning overload for side-effecting operations.
- **`DatabaseError`** — Sealed record extending `Error` with `SqlState` and `ErrorNumber` properties for provider-agnostic database error info.
- **`DatabaseExceptionMapper`** — Internal helper mapping `DbException` → `DatabaseError` (preserving `SqlState`/`ErrorCode`), `TimeoutException` → `DatabaseError` with code `"TIMEOUT"`.

### Entity Framework Core Extensions (DarkPeak.Functional.EntityFramework)

- **`DbContextExtensions`** — Extension methods wrapping EF Core operations in `Result<T, Error>`:
  - `SaveChangesResultAsync()` — Save changes, returns `Result<int, Error>`
  - `FindResultAsync<T>()` — Find by primary key, returns `Result<Option<T>, Error>`
  - `FirstOrDefaultResultAsync<T>()` — Extension on `IQueryable<T>`, returns `Result<Option<T>, Error>`
  - `SingleOrDefaultResultAsync<T>()` — Extension on `IQueryable<T>`, returns `Result<Option<T>, Error>`
  - `FirstResultAsync<T>()` — Extension on `IQueryable<T>`, fails on empty
  - `SingleResultAsync<T>()` — Extension on `IQueryable<T>`, fails on empty or multiple
  - `ToListResultAsync<T>()` — Materialize query to `Result<List<T>, Error>`
  - `CountResultAsync<T>()` — Count query results
  - `AnyResultAsync<T>()` — Check if any results exist
- **`DbContextTransactionExtensions`** — Functional transaction support:
  - `ExecuteInTransactionAsync<T>()` — Executes a delegate within an EF Core transaction; commits on `Success`, rolls back on `Failure` or exception. Caller must call `SaveChangesAsync` within the delegate. Supports `CancellationToken`.
  - `ExecuteInTransactionAsync()` — Unit-returning overload.
- **Error types**:
  - `EntityFrameworkError` — Base record for EF Core errors
  - `ConcurrencyError` — Wraps `DbUpdateConcurrencyException`, includes `ConflictingEntries` (entity type names)
  - `SaveChangesError` — Wraps `DbUpdateException`, includes `SqlState` and `AffectedEntries`
- **Exception mapping**: `DbUpdateConcurrencyException` → `ConcurrencyError`, `DbUpdateException` → `SaveChangesError`, `OperationCanceledException` → `EntityFrameworkError` (code `"CANCELLED"`), other → `EntityFrameworkError` (code `"UNKNOWN"`)

### Mediator Integration (DarkPeak.Functional.Mediator)

Integrates with the [Mediator](https://github.com/martinothamar/Mediator) source-generated library for CQRS patterns.

- **Convenience interfaces** — Eliminate verbose `Result<T, Error>` generic arguments:
  - `IResultCommand<T>` — Command returning `Result<T, Error>`. `IResultCommand` returns `Result<Unit, Error>`.
  - `IResultQuery<T>` — Query returning `Result<T, Error>`
  - `IResultRequest<T>` — Request returning `Result<T, Error>`. `IResultRequest` returns `Result<Unit, Error>`.
- **`IValidate`** — Self-validating message interface with `bool IsValid(out ValidationError? error)`. Implement on commands/queries to enable automatic validation.
- **`ResultValidationBehavior<TMessage, T>`** — `IPipelineBehavior` that short-circuits invalid messages (those implementing `IValidate`) with a `Failure` containing the `ValidationError`.
- **`ResultExceptionHandler<TMessage, T>`** — `MessageExceptionHandler` that catches unhandled exceptions and converts them to `Failure` with `InternalError`, preventing exceptions from escaping the pipeline.
- **`SenderExtensions`** — Typed `SendResult()` extension methods on `ISender` for `IResultCommand<T>`, `IResultCommand`, `IResultQuery<T>`, `IResultRequest<T>`, and `IResultRequest`.
- **`ServiceCollectionExtensions`** — `AddDarkPeakMediatorBehaviors()` registers both pipeline behaviors in recommended order: exception handler (outermost) → validation (innermost).

### Redis Cache Provider (DarkPeak.Functional.Redis)

- **`RedisCacheProvider<TKey, TValue>`** — Implements `ICacheProvider<TKey, TValue>` backed by StackExchange.Redis `IDatabase`. Serializes values with `System.Text.Json`, converts keys to strings via `ToString()`. Supports optional key prefix for namespace isolation and custom `JsonSerializerOptions`. Constructor: `RedisCacheProvider(IDatabase database, string? keyPrefix = null, JsonSerializerOptions? jsonOptions = null)`.

### HTTP Client Extensions (DarkPeak.Functional.Http)

- **`HttpClientExtensions`** — Extension methods on `HttpClient` that wrap HTTP operations in `Result<T, Error>`:
  - `GetResultAsync<T>()` — GET with JSON deserialization
  - `PostResultAsync<T>()` — POST with JSON body
  - `PutResultAsync<T>()` — PUT with JSON body
  - `PatchResultAsync<T>()` — PATCH with JSON body
  - `DeleteResultAsync()` — DELETE returning `Result<Unit, Error>`
  - `DeleteResultAsync<T>()` — DELETE with JSON response body
  - `SendResultAsync<T>()` — Custom `HttpRequestMessage` with JSON response
  - `SendResultAsync()` — Custom `HttpRequestMessage` without response body
  - `GetStringResultAsync()` — GET returning `Result<string, Error>` for plain text/XML/HTML
  - `GetStreamResultAsync()` — GET returning `Result<Stream, Error>` with `ResponseHeadersRead` for streaming
  - `GetBytesResultAsync()` — GET returning `Result<byte[], Error>` for binary data
  - All methods above (except `SendResultAsync`) have an overload accepting `Action<HttpRequestMessage> configure` for per-request header/auth customization
- **`HttpError`** — Error type for HTTP responses with `StatusCode`, `ReasonPhrase`, `ResponseBody`
- **`HttpRequestError`** — Error type for transport-level failures (network errors, timeouts)
- **Status code mapping**: 400→BadRequestError, 401→UnauthorizedError, 403→ForbiddenError, 404→NotFoundError, 409→ConflictError, 422→ValidationError, 5xx→ExternalServiceError, other→HttpError

### ASP.NET Integration (DarkPeak.Functional.AspNet)

- **`ResultExtensions`** — Extension methods for converting `Result<T, Error>` to ASP.NET `IResult`:
  - `ToIResult()` — Maps success to `200 OK`, errors to typed HTTP responses
  - `ToCreatedResult()` — Maps success to `201 Created` with location header
  - `ToNoContentResult()` — Maps success to `204 No Content`
  - Async overloads for all methods accepting `Task<Result<T, Error>>`
- **`ProblemDetailsExtensions`** — Extension methods for converting `Error` to RFC 9457 ProblemDetails:
  - `ToProblemDetails()` — Converts any `Error` to `ProblemDetails` with correct status code
  - `ToValidationProblemDetails()` — Converts `ValidationError` to `HttpValidationProblemDetails` preserving field errors
  - Metadata and error codes are included in `ProblemDetails.Extensions`

### Health Check Integration (DarkPeak.Functional.HealthChecks)

- **`CircuitBreakerHealthCheck`** — `IHealthCheck` that monitors a `CircuitBreakerPolicy`. Maps Closed→Healthy, HalfOpen→Degraded, Open→Unhealthy. Includes `state`, `failureCount`, and `resetTime` in health check data.
- **`CacheProviderHealthCheck<TKey, TValue>`** — `IHealthCheck` that probes an `ICacheProvider<TKey, TValue>` by calling `GetAsync` with a sentinel key. Healthy if reachable, Unhealthy if it throws.
- **`CircuitBreakerSnapshot`** — Immutable record exposing circuit breaker state: `State`, `ConsecutiveFailures`, `LastFailureTime`, `ResetTimeout`. Obtained via `CircuitBreakerPolicy.GetSnapshot()`.
- **`HealthChecksBuilderExtensions`** — Fluent registration on `IHealthChecksBuilder`:
  - `AddCircuitBreakerHealthCheck(name, policy, tags?)` — Registers a circuit breaker health check
  - `AddCacheProviderHealthCheck<TKey, TValue>(name, cacheProvider, probeKey, tags?)` — Registers a cache health check that probes an `ICacheProvider` with `GetAsync`

### Extensions Namespace (`DarkPeak.Functional.Extensions`)

- **`TypeConversionExtensions`** — Cross-type conversions (Option↔Result↔Either)
- **`TaskOptionExtensions`** / **`TaskResultExtensions`** / **`TaskValidationExtensions`** — Fluent async chaining on `Task<Option<T>>`, `Task<Result<T, TError>>`, and `Task<Validation<T, TError>>`. Includes concurrent `Join` (arities 2-8), `ZipWithAsync` (Validation, arities 2-8), `SequenceAsync`, `TraverseAsync`, `SequenceParallel`, and `TraverseParallel`
- **`EitherExtensions`** — GetLeftOrDefault, GetRightOrDefault, Flatten, Merge, Partition
- **`ValidationExtensions`** — Apply (error accumulation), ZipWith (applicative combinator), Sequence, Traverse, Join (error-accumulating), ToResult/ToValidation interop
- **`OptionExtensions`** / **`ResultExtensions`** — Collection operations (Choose, Partition, Sequence, Traverse, Join), async variants (SequenceAsync, TraverseAsync, PartitionAsync, ChooseAsync), parallel variants (SequenceParallel, TraverseParallel, PartitionParallel, ChooseParallel)
- **`Pipeline`** — Pipe/Compose extensions for ad-hoc function composition, fluent `Pipeline.Create<TInput, TError>()` builder for reusable workflows with sync/async steps

### Type Hierarchy Pattern

Each monadic type follows this structure:
1. Abstract base record defining the interface (e.g., `Option<T>`)
2. Concrete sealed records for each case (e.g., `Some<T>`, `None<T>`)
3. Static factory class for convenient construction (e.g., `Option.Some()`, `Option.None<T>()`)
4. Extension methods in the `Extensions` namespace for additional operations

## Key Conventions

### Exhaustive Pattern Matching
Use `Match()` to handle all cases explicitly rather than checking `IsSome`/`IsSuccess`/`IsLeft` and casting:
```csharp
var result = option.Match(
    some: value => $"Got {value}",
    none: () => "Nothing"
);
```

### LINQ Support
All monadic types support LINQ query syntax via `Select`, `SelectMany`, and `Where`:
```csharp
var result = from x in option
             from y in Option.Some(x * 2)
             where y > 5
             select y;
```

### Error Type Constraint
`Result<T, TError>` requires `TError : Error`. Create domain-specific errors by inheriting from `Error` or using provided types like `ValidationError`, `NotFoundError`.

### Async Variants
All operations have async counterparts suffixed with `Async` (e.g., `Map` → `MapAsync`, `Bind` → `BindAsync`).

### Testing Framework
Tests use TUnit (`[Test]` attribute) with async assertions (`await Assert.That(...)`). The solution uses Microsoft.Testing.Platform via `global.json` opt-in, so tests are run with `dotnet test --solution DarkPeak.Functional.slnx`. Code coverage is collected via the built-in `Microsoft.Testing.Extensions.CodeCoverage` (transitive from TUnit) using the `--coverage` flag.

### Commit Messages
This project uses [Conventional Commits](https://www.conventionalcommits.org/) (e.g., `feat:`, `fix:`, `docs:`, `test:`, `chore:`). Versions are derived automatically by GitVersion.

## CI/CD

- **GitVersion** (TrunkBased mode) derives SemVer from conventional commits
- **CI workflow** (`ci.yml`) builds, tests, packs, and uploads artifacts on every push to `main` and every PR — but does NOT publish or release
- **Release workflow** (`release.yml`) is triggered manually via `workflow_dispatch`. It builds, tests, packs all libraries (iterates `src/*/`) with the same version, publishes to NuGet, creates a git tag, and creates a GitHub Release. A `prerelease` input controls whether the release is marked as a prerelease.
- **Docs workflow** (`docs.yml`) builds and deploys DocFX API documentation to GitHub Pages
- **PR Labeler workflow** (`pr-labeler.yml`) auto-labels PRs based on conventional commit title prefixes (e.g., `feat:` → `enhancement`, `fix:` → `bug`, `docs:` → `documentation`, `!` suffix → `breaking`)
- **Dependabot** monitors GitHub Actions weekly and NuGet production dependencies (ignoring minor/patch updates for ASP.NET and System.Net.Http)
- **NuGet** packages published to nuget.org (requires `NUGET_API_KEY` secret)
- **Directory.Build.props** at the repo root provides shared build configuration (target framework, nullable, NuGet metadata) for all projects
