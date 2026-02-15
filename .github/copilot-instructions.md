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

This is a .NET 10 functional programming library providing monadic types for railway-oriented programming. The solution is composed of three packable libraries and their corresponding test projects. The library targets nullable-enabled C# with strict null reference handling.

### Solution Structure

```
DarkPeak.Functional/
├── Directory.Build.props           # Shared build properties (TFM, nullable, NuGet metadata)
├── GitVersion.yml                  # Version calculation from conventional commits
├── src/
│   ├── DarkPeak.Functional/        # Core library: monadic types and extensions
│   ├── DarkPeak.Functional.Http/   # HTTP client extensions wrapping HttpClient in Result<T, Error>
│   └── DarkPeak.Functional.AspNet/ # ASP.NET integration (ToIResult, ProblemDetails)
├── tests/
│   ├── DarkPeak.Functional.Tests/
│   ├── DarkPeak.Functional.Http.Tests/
│   └── DarkPeak.Functional.AspNet.Tests/
└── docs/                           # DocFX API documentation
```

### Package Dependencies

- **DarkPeak.Functional** — No external dependencies. The core library.
- **DarkPeak.Functional.Http** — Depends on `DarkPeak.Functional`. Provides `HttpClientExtensions` for wrapping `HttpClient` operations in `Result<T, Error>`.
- **DarkPeak.Functional.AspNet** — Depends on `DarkPeak.Functional`. References `Microsoft.AspNetCore.App` shared framework. Provides `ToIResult()`, `ToCreatedResult()`, `ToNoContentResult()`, and `ToProblemDetails()` extensions.

All three packages are versioned together and released simultaneously via the manual Release workflow.

### Core Types (DarkPeak.Functional)

- **`Option<T>`** — Represents a value that may or may not exist (alternative to null). Implementations: `Some<T>`, `None<T>`
- **`Result<T, TError>`** — Represents an operation that can succeed with a value or fail with a typed error. Implementations: `Success<T, TError>`, `Failure<T, TError>`
- **`Either<TLeft, TRight>`** — Represents a value that can be one of two types (both valid states, not success/failure). Implementations: `Left<TLeft, TRight>`, `Right<TLeft, TRight>`
- **`Validation<T, TError>`** — Accumulates multiple errors instead of short-circuiting. Implementations: `Valid<T, TError>`, `Invalid<T, TError>`
- **`Error`** — Abstract base record for typed errors with HTTP-mapped subtypes (ValidationError, NotFoundError, UnauthorizedError, etc.)
- **`RetryPolicy`** — Configurable retry with backoff strategies (None, Constant, Linear, Exponential). Entry point: `Retry.WithMaxAttempts()`
- **`CircuitBreakerPolicy`** — Circuit breaker that tracks consecutive failures and transitions between Closed, Open, and HalfOpen states to prevent cascading failures. Entry point: `CircuitBreaker.WithFailureThreshold()`. Uses a shared `CircuitBreakerStateTracker` for mutable state with `Lock`-based thread safety.
- **`CircuitBreakerOpenError`** — Error returned when the circuit is open, with a `RetryAfter` property indicating time until half-open.
- **`Memoize`** — Function caching with TTL, LRU eviction, and pluggable distributed cache via `ICacheProvider<TKey, TValue>`
- **`MemoizeResult`** — Function caching for `Result<T, TError>`-returning functions that only caches successful results. Failed results pass through uncached so subsequent calls retry the computation. Entry point: `MemoizeResult.Func()` / `MemoizeResult.FuncAsync()`
- **`Unit`** — Valueless type representing a successful operation with no return value. Located in the core `DarkPeak.Functional` namespace.

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
- **`Unit`** — Now located in the core `DarkPeak.Functional` namespace (moved from Http)
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

### Extensions Namespace (`DarkPeak.Functional.Extensions`)

- **`TypeConversionExtensions`** — Cross-type conversions (Option↔Result↔Either)
- **`TaskOptionExtensions`** / **`TaskResultExtensions`** — Fluent async chaining on `Task<Option<T>>` and `Task<Result<T, TError>>`
- **`EitherExtensions`** — GetLeftOrDefault, GetRightOrDefault, Flatten, Merge, Partition
- **`ValidationExtensions`** — Apply (error accumulation), Combine, Sequence, ToResult/ToValidation interop
- **`OptionExtensions`** / **`ResultExtensions`** — Collection operations (Choose, Partition, etc.)

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
- **Release workflow** (`release.yml`) is triggered manually via `workflow_dispatch`. It builds, tests, packs all 3 libraries with the same version, publishes to NuGet, creates a git tag, and creates a GitHub Release. A `prerelease` input controls whether the release is marked as a prerelease.
- **Docs workflow** (`docs.yml`) builds and deploys DocFX API documentation to GitHub Pages
- **Dependabot** monitors GitHub Actions weekly and NuGet production dependencies (ignoring minor/patch updates for ASP.NET and System.Net.Http)
- **NuGet** packages published to nuget.org (requires `NUGET_API_KEY` secret)
- **Directory.Build.props** at the repo root provides shared build configuration (target framework, nullable, NuGet metadata) for all projects
