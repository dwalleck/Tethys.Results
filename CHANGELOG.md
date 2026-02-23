# Changelog

All notable changes to the Tethys.Results package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2026-02-23

### Added
- **`Then` extension** for `Result<TValue, TError>`: Monadic bind that chains operations on success, short-circuits on failure. Supports same-type and type-changing chains (e.g., `Result<int, E>` → `Result<string, E>`)
- **`When` extension** for `Result<TValue, TError>`: Conditional bind — executes the operation only if the result is successful and the condition is true
- **`Map` extension** for `Result<TValue, TError>`: Transforms the success value without requiring the caller to wrap in a `Result` (functor map)
- **`ThenAsync` extension** for `Result<TValue, TError>`: Async bind with overloads for both `Result` and `Task<Result>` sources
- **`WhenAsync` extension** for `Result<TValue, TError>`: Async conditional bind with overloads for both `Result` and `Task<Result>` sources
- **`MapAsync` extension** for `Result<TValue, TError>`: Async value transformation with overloads for both `Result` and `Task<Result>` sources
- **Null guards** on all delegate parameters — all extension methods throw `ArgumentNullException` for null callbacks, consistent with `Result<TValue, TError>.Match`/`MatchAsync`
- **`Value` property** on `Result<T>`: Alias for the `Data` property, providing API consistency with `Result<TValue, TError>.Value`

### Deprecated
- **`Result<T>.Data`** — use `Value` instead. `Data` will be removed in a future major version.

### Fixed
- **Missing `ConfigureAwait(false)`** on all async `await` calls in `ResultExtensions.cs` (existing `Result`/`Result<T>` extensions) — prevents potential deadlocks when callers use `.Result` or `.Wait()` with a `SynchronizationContext`

### Developer Notes
- All new extension methods placed in `TypedResultExtensions.cs` to keep the two result families separated
- Added 69 new tests covering sync, async, pipeline integration, short-circuit behavior, Task-source overloads, null-guard validation, exception propagation, null values, faulted/canceled tasks, delegate-throws, and operation-returns-failure scenarios
- Total test count increased from 393 to 462
- All internal code and tests migrated from `Data` to `Value`; `Data` remains as an `[Obsolete]` alias for backward compatibility
- Used unified 3-type-parameter `Then<TValue, TNewValue, TError>` to avoid C# overload ambiguity when `TValue == TNewValue`

## [1.3.0] - 2026-02-23

### Added
- **`Success<T>` wrapper struct**: Lightweight value type that tags a value as a success for unambiguous implicit conversion to `Result<TValue, TError>`
- **`Failure<T>` wrapper struct**: Lightweight value type that tags a value as a failure for unambiguous implicit conversion to `Result<TValue, TError>`
- **`Result.Ok<T>(T value)` factory method**: Creates a `Success<T>` wrapper for use with implicit conversions
- **`Result.Fail<T>(T error)` factory method**: Creates a `Failure<T>` wrapper for use with implicit conversions
- Implicit operators on `Result<TValue, TError>` from `Success<TValue>` and `Failure<TError>`, enabling `return Result.Ok(value)` and `return Result.Fail(error)` in methods returning `Result<TValue, TError>`

### Fixed
- **CS0457 compiler error** when `TValue` and `TError` are the same type (e.g., `Result<string, string>`) — wrapper pattern eliminates the ambiguity that direct implicit operators would cause
- **Inheritance ambiguity** with assignable types (e.g., `Result<object, string>`) — `Result.Ok(value)` and `Result.Fail(error)` make intent explicit at every call site

### Developer Notes
- Added 49 new tests for wrapper-based implicit conversions
- Total test count increased from 344 to 393
- All existing `Result<TValue, TError>.Ok()` and `.Fail()` static methods remain unchanged

## [1.2.0] - 2026-02-09

### Added
- **`Result<TValue, TError>` type**: New typed result class for operations that return a specific success value or a specific error type, enabling strongly-typed error handling without exceptions
  - `Ok(TValue value)` - Creates a successful result with a value
  - `Fail(TError error)` - Creates a failed result with a typed error
  - `Match<TResult>` - Pattern matching for success/failure handling
  - `MatchAsync<TResult>` - Async pattern matching
  - Full equality support (`IEquatable<T>`, `==`, `!=`, `GetHashCode`)
  - Thread-safe, immutable implementation

### Developer Notes
- Added 28 new tests for `Result<TValue, TError>`
- Total test count increased from 316 to 344
- Maintained 100% backward compatibility with existing `Result` and `Result<T>` types

## [1.1.0] - 2025-07-05

### Added
- **Equality Support**: Both `Result` and `Result<T>` now implement `IEquatable<T>` with proper `Equals`, `GetHashCode`, and equality operators (`==`, `!=`)
- **Functional Programming Methods**:
  - `Match<TResult>` - Pattern matching for elegant success/failure handling
  - `Map<TNew>` - Transform success values while preserving failures
  - `FlatMap<TNew>` - Chain operations that return Results (monadic bind)
  - `MapError` - Transform error messages/exceptions
  - All methods include async versions (`MatchAsync`, `MapAsync`, etc.)
- **Try Pattern Support**:
  - `Result.Try()` - Wrap exception-throwing operations in Result pattern
  - `Result<T>.Try()` - Capture return values from exception-throwing functions
  - Async versions with `TryAsync`
  - Support for custom success/error messages
- **Callback Methods**:
  - `OnSuccess` - Execute side effects only on successful results
  - `OnFailure` - Execute side effects only on failed results
  - `OnBoth` - Execute side effects regardless of result state (useful for logging)
  - All callbacks include async versions and preserve the original result for chaining

### Fixed
- Updated repository URL from placeholder to actual GitHub repository

### Changed
- **Documentation**: Complete overhaul of documentation
  - Replaced placeholder README.md with comprehensive package documentation
  - Added detailed getting started guide (`docs/getting-started.md`)
  - Added complete API reference (`docs/api-reference.md`)
  - Added extensive examples and patterns guide (`docs/examples.md`)
- Package version incremented to 1.1.0

### Developer Notes
- All new features developed using Test-Driven Development (TDD)
- Added 159 new tests for the new features
- Total test count increased from 157 to 316
- Maintained 100% backward compatibility
- All public APIs include comprehensive XML documentation

## [1.0.1] - 2025-07-05

### Fixed
- Documentation cleanup and package metadata corrections

## [1.0.0] - 2025-04-01

### Added
- Initial release of Tethys.Results
- IResult interface defining the common contract for all result types
- Result class for non-generic operation results
- Result<T> class for generic operation results with data
- AggregateError class for combining multiple errors
- Extension methods for working with results
- Comprehensive XML documentation
- Full test coverage
