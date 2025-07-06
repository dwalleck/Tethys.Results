# Changelog

All notable changes to the Tethys.Results package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
