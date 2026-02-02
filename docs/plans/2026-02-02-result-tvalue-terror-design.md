# Result<TValue, TError> Design

**Date:** 2026-02-02
**Status:** Approved

## Problem

The existing `Result<T>` in Tethys.Results only supports `Exception`-based errors. For scenarios like API controllers that need to map different error types to different HTTP status codes, typed domain errors are required.

**Current limitation:**
```csharp
Result<TransHistory> result = LoadHistory(id);
// Can't easily map string/exception errors to HTTP status codes
```

**Desired:**
```csharp
Result<TransHistory, LoadError> result = LoadHistory(id);
return result.Match(
    onSuccess: history => Ok(history),
    onFailure: error => error switch {
        NotFoundError => NotFound(),
        ValidationError => BadRequest(),
        _ => StatusCode(500)
    }
);
```

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Relationship to existing types | Completely separate | Different use cases (exception-based vs domain-error-based); shared interface adds complexity without benefit |
| TError constraints | None | Maximum flexibility - allows enums, classes, structs, strings |
| API surface | Minimal MVP | Start lean, expand based on real needs; methods can be added later (non-breaking) |
| Invalid access behavior | Throw exception | Fail-fast; forces correct usage via Match() or checking Success first |
| Async support | Include MatchAsync | Essential for real-world async controller/service scenarios |

## Implementation

### File Location

`src/Tethys.Results/Result{TValue,TError}.cs`

### Complete Implementation

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tethys.Results
{
    /// <summary>
    /// Represents the result of an operation that returns either a value of type
    /// <typeparamref name="TValue"/> on success, or an error of type
    /// <typeparamref name="TError"/> on failure.
    /// This class is immutable and thread-safe.
    /// </summary>
    /// <typeparam name="TValue">The type of value returned on success.</typeparam>
    /// <typeparam name="TError">The type of error returned on failure.</typeparam>
    public sealed class Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; }

        private readonly TValue _value;
        private readonly TError _error;

        private Result(bool success, TValue value, TError error)
        {
            Success = success;
            _value = value;
            _error = error;
        }

        // ===== Factory Methods =====

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A successful result containing the value.</returns>
        public static Result<TValue, TError> Ok(TValue value)
        {
            return new Result<TValue, TError>(true, value, default);
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A failed result containing the error.</returns>
        public static Result<TValue, TError> Fail(TError error)
        {
            return new Result<TValue, TError>(false, default, error);
        }

        // ===== Property Accessors =====

        /// <summary>
        /// Gets the success value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessing Value on a failed result.
        /// </exception>
        public TValue Value
        {
            get
            {
                if (!Success)
                {
                    throw new InvalidOperationException(
                        "Cannot access Value on a failed result. Check Success first or use Match().");
                }
                return _value;
            }
        }

        /// <summary>
        /// Gets the error value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessing Error on a successful result.
        /// </exception>
        public TError Error
        {
            get
            {
                if (Success)
                {
                    throw new InvalidOperationException(
                        "Cannot access Error on a successful result. Check Success first or use Match().");
                }
                return _error;
            }
        }

        // ===== Pattern Matching =====

        /// <summary>
        /// Pattern matches on the result, executing the appropriate function
        /// based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of value returned by both functions.</typeparam>
        /// <param name="onSuccess">Function to execute if successful, receives the value.</param>
        /// <param name="onFailure">Function to execute if failed, receives the error.</param>
        /// <returns>The value returned by the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<TError, TResult> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return Success ? onSuccess(_value) : onFailure(_error);
        }

        /// <summary>
        /// Asynchronously pattern matches on the result, executing the appropriate function
        /// based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of value returned by both functions.</typeparam>
        /// <param name="onSuccess">Async function to execute if successful, receives the value.</param>
        /// <param name="onFailure">Async function to execute if failed, receives the error.</param>
        /// <returns>A task containing the value returned by the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
        public async Task<TResult> MatchAsync<TResult>(
            Func<TValue, Task<TResult>> onSuccess,
            Func<TError, Task<TResult>> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return Success
                ? await onSuccess(_value).ConfigureAwait(false)
                : await onFailure(_error).ConfigureAwait(false);
        }

        // ===== Equality =====

        /// <summary>
        /// Determines whether the specified result is equal to this instance.
        /// </summary>
        /// <param name="other">The result to compare with this instance.</param>
        /// <returns>true if the results are equal; otherwise, false.</returns>
        public bool Equals(Result<TValue, TError> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Success != other.Success)
                return false;

            return Success
                ? EqualityComparer<TValue>.Default.Equals(_value, other._value)
                : EqualityComparer<TError>.Default.Equals(_error, other._error);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Result<TValue, TError>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Success.GetHashCode();
                hashCode = (hashCode * 397) ^ (Success
                    ? EqualityComparer<TValue>.Default.GetHashCode(_value)
                    : EqualityComparer<TError>.Default.GetHashCode(_error));
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether two results are equal.
        /// </summary>
        public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are not equal.
        /// </summary>
        public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right)
        {
            return !(left == right);
        }
    }
}
```

## Test Coverage Required

1. **Factory method tests**
   - `Ok()` creates successful result with value
   - `Fail()` creates failed result with error

2. **Property accessor tests**
   - `Value` returns value on success
   - `Value` throws on failure
   - `Error` returns error on failure
   - `Error` throws on success

3. **Match tests**
   - Executes `onSuccess` for successful result
   - Executes `onFailure` for failed result
   - Throws on null callbacks

4. **MatchAsync tests**
   - Async version of Match tests

5. **Equality tests**
   - Equal successful results
   - Equal failed results
   - Unequal results (different Success)
   - Unequal results (different Value/Error)
   - Null handling
   - Operator == and !=

6. **Edge cases**
   - Works with value types for TValue/TError
   - Works with reference types for TValue/TError
   - Works with null values (for reference types)

## Future Considerations

Methods that could be added later based on usage:

- `Map<TNewValue>(Func<TValue, TNewValue>)` - transform success value
- `MapError<TNewError>(Func<TError, TNewError>)` - transform error type
- `FlatMap<TNewValue>(Func<TValue, Result<TNewValue, TError>>)` - monadic bind
- `GetValueOrDefault(TValue defaultValue)` - safe extraction
- `TryGetValue(out TValue value)` - try pattern
- Implicit conversions
