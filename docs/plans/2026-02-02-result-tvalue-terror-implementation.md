# Result<TValue, TError> Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a `Result<TValue, TError>` type enabling typed domain errors for scenarios like API controllers mapping errors to HTTP status codes.

**Architecture:** New standalone sealed class with private constructor, factory methods (Ok/Fail), throwing property accessors, and Match/MatchAsync for safe pattern matching. Completely separate from existing `Result<T>` - no shared interface.

**Tech Stack:** C# / .NET Standard 2.0 / TUnit for tests

---

## Task 1: Create Test File and First Failing Test (Factory Methods)

**Files:**
- Create: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Create test file with Ok factory method test**

```csharp
using System;
using System.Threading.Tasks;
using Tethys.Results;

namespace Tethys.Test
{
    public class TypedResultTests
    {
        [Test]
        public async Task Ok_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var value = "test value";

            // Act
            var result = Result<string, string>.Ok(value);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(value);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test --filter "Ok_ShouldCreateSuccessfulResult"`
Expected: FAIL - `Result<TValue, TError>` type does not exist

**Step 3: Commit failing test**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add failing test for Result<TValue, TError>.Ok"
```

---

## Task 2: Create Result<TValue, TError> Class with Ok Factory

**Files:**
- Create: `src/Tethys.Results/TypedResult.cs`

**Step 1: Create minimal implementation to pass the test**

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

        // Equality stub - will implement fully later
        public bool Equals(Result<TValue, TError> other) => false;
    }
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test --filter "Ok_ShouldCreateSuccessfulResult"`
Expected: PASS

**Step 3: Commit passing implementation**

```bash
git add src/Tethys.Results/TypedResult.cs
git commit -m "feat: add Result<TValue, TError> with Ok factory method"
```

---

## Task 3: Add Fail Factory Method Test

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add test for Fail factory method**

Add this test to TypedResultTests class:

```csharp
[Test]
public async Task Fail_ShouldCreateFailedResult()
{
    // Arrange
    var error = "validation error";

    // Act
    var result = Result<string, string>.Fail(error);

    // Assert
    await Assert.That(result.Success).IsFalse();
    await Assert.That(result.Error).IsEqualTo(error);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test --filter "Fail_ShouldCreateFailedResult"`
Expected: FAIL - `Fail` method and `Error` property do not exist

**Step 3: Commit failing test**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add failing test for Result<TValue, TError>.Fail"
```

---

## Task 4: Implement Fail Factory and Error Property

**Files:**
- Modify: `src/Tethys.Results/TypedResult.cs`

**Step 1: Add Fail method and Error property**

Add to the Result<TValue, TError> class:

```csharp
/// <summary>
/// Creates a failed result with the specified error.
/// </summary>
/// <param name="error">The error value.</param>
/// <returns>A failed result containing the error.</returns>
public static Result<TValue, TError> Fail(TError error)
{
    return new Result<TValue, TError>(false, default, error);
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
```

**Step 2: Run test to verify it passes**

Run: `dotnet test --filter "Fail_ShouldCreateFailedResult"`
Expected: PASS

**Step 3: Commit passing implementation**

```bash
git add src/Tethys.Results/TypedResult.cs
git commit -m "feat: add Fail factory method and Error property"
```

---

## Task 5: Add Property Accessor Throwing Tests

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add tests for throwing behavior**

Add these tests:

```csharp
[Test]
public async Task Value_OnFailedResult_ShouldThrowInvalidOperationException()
{
    // Arrange
    var result = Result<string, string>.Fail("error");

    // Act & Assert
    await Assert.That(() => _ = result.Value)
        .Throws<InvalidOperationException>()
        .WithMessage("Cannot access Value on a failed result. Check Success first or use Match().");
}

[Test]
public async Task Error_OnSuccessfulResult_ShouldThrowInvalidOperationException()
{
    // Arrange
    var result = Result<string, string>.Ok("value");

    // Act & Assert
    await Assert.That(() => _ = result.Error)
        .Throws<InvalidOperationException>()
        .WithMessage("Cannot access Error on a successful result. Check Success first or use Match().");
}
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "Value_OnFailedResult_ShouldThrowInvalidOperationException|Error_OnSuccessfulResult_ShouldThrowInvalidOperationException"`
Expected: PASS (already implemented)

**Step 3: Commit tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add throwing behavior tests for Value and Error properties"
```

---

## Task 6: Add Match Method Test

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add test for Match on success**

```csharp
[Test]
public async Task Match_OnSuccess_ShouldExecuteOnSuccessFunction()
{
    // Arrange
    var result = Result<int, string>.Ok(42);

    // Act
    var output = result.Match(
        onSuccess: value => $"Value: {value}",
        onFailure: error => $"Error: {error}"
    );

    // Assert
    await Assert.That(output).IsEqualTo("Value: 42");
}

[Test]
public async Task Match_OnFailure_ShouldExecuteOnFailureFunction()
{
    // Arrange
    var result = Result<int, string>.Fail("not found");

    // Act
    var output = result.Match(
        onSuccess: value => $"Value: {value}",
        onFailure: error => $"Error: {error}"
    );

    // Assert
    await Assert.That(output).IsEqualTo("Error: not found");
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "Match_On"`
Expected: FAIL - `Match` method does not exist

**Step 3: Commit failing tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add failing tests for Match method"
```

---

## Task 7: Implement Match Method

**Files:**
- Modify: `src/Tethys.Results/TypedResult.cs`

**Step 1: Add Match method**

```csharp
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
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "Match_On"`
Expected: PASS

**Step 3: Commit passing implementation**

```bash
git add src/Tethys.Results/TypedResult.cs
git commit -m "feat: add Match method for pattern matching"
```

---

## Task 8: Add Match Null Argument Tests

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add null argument tests**

```csharp
[Test]
public async Task Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
{
    // Arrange
    var result = Result<int, string>.Ok(42);

    // Act & Assert
    await Assert.That(() => result.Match<string>(null, error => error))
        .Throws<ArgumentNullException>();
}

[Test]
public async Task Match_WithNullOnFailure_ShouldThrowArgumentNullException()
{
    // Arrange
    var result = Result<int, string>.Ok(42);

    // Act & Assert
    await Assert.That(() => result.Match<string>(value => value.ToString(), null))
        .Throws<ArgumentNullException>();
}
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "Match_WithNull"`
Expected: PASS (already implemented)

**Step 3: Commit tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add null argument tests for Match method"
```

---

## Task 9: Add MatchAsync Tests

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add async match tests**

```csharp
[Test]
public async Task MatchAsync_OnSuccess_ShouldExecuteOnSuccessFunction()
{
    // Arrange
    var result = Result<int, string>.Ok(42);

    // Act
    var output = await result.MatchAsync(
        onSuccess: async value => { await Task.Delay(1); return $"Value: {value}"; },
        onFailure: async error => { await Task.Delay(1); return $"Error: {error}"; }
    );

    // Assert
    await Assert.That(output).IsEqualTo("Value: 42");
}

[Test]
public async Task MatchAsync_OnFailure_ShouldExecuteOnFailureFunction()
{
    // Arrange
    var result = Result<int, string>.Fail("not found");

    // Act
    var output = await result.MatchAsync(
        onSuccess: async value => { await Task.Delay(1); return $"Value: {value}"; },
        onFailure: async error => { await Task.Delay(1); return $"Error: {error}"; }
    );

    // Assert
    await Assert.That(output).IsEqualTo("Error: not found");
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "MatchAsync_On"`
Expected: FAIL - `MatchAsync` method does not exist

**Step 3: Commit failing tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add failing tests for MatchAsync method"
```

---

## Task 10: Implement MatchAsync Method

**Files:**
- Modify: `src/Tethys.Results/TypedResult.cs`

**Step 1: Add MatchAsync method**

```csharp
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
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "MatchAsync_On"`
Expected: PASS

**Step 3: Commit passing implementation**

```bash
git add src/Tethys.Results/TypedResult.cs
git commit -m "feat: add MatchAsync method for async pattern matching"
```

---

## Task 11: Add Equality Tests

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add equality tests**

```csharp
[Test]
public async Task Equals_TwoSuccessfulResultsWithSameValue_ShouldBeEqual()
{
    // Arrange
    var result1 = Result<int, string>.Ok(42);
    var result2 = Result<int, string>.Ok(42);

    // Assert
    await Assert.That(result1.Equals(result2)).IsTrue();
    await Assert.That(result1 == result2).IsTrue();
    await Assert.That(result1 != result2).IsFalse();
}

[Test]
public async Task Equals_TwoFailedResultsWithSameError_ShouldBeEqual()
{
    // Arrange
    var result1 = Result<int, string>.Fail("error");
    var result2 = Result<int, string>.Fail("error");

    // Assert
    await Assert.That(result1.Equals(result2)).IsTrue();
    await Assert.That(result1 == result2).IsTrue();
}

[Test]
public async Task Equals_SuccessAndFailure_ShouldNotBeEqual()
{
    // Arrange
    var success = Result<int, string>.Ok(42);
    var failure = Result<int, string>.Fail("error");

    // Assert
    await Assert.That(success.Equals(failure)).IsFalse();
    await Assert.That(success == failure).IsFalse();
    await Assert.That(success != failure).IsTrue();
}

[Test]
public async Task Equals_WithNull_ShouldReturnFalse()
{
    // Arrange
    var result = Result<int, string>.Ok(42);

    // Assert
    await Assert.That(result.Equals(null)).IsFalse();
    await Assert.That(result == null).IsFalse();
    await Assert.That(null == result).IsFalse();
}

[Test]
public async Task GetHashCode_EqualResults_ShouldHaveSameHashCode()
{
    // Arrange
    var result1 = Result<int, string>.Ok(42);
    var result2 = Result<int, string>.Ok(42);

    // Assert
    await Assert.That(result1.GetHashCode()).IsEqualTo(result2.GetHashCode());
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "Equals_|GetHashCode_"`
Expected: FAIL - Equals returns false (stub), operators don't exist

**Step 3: Commit failing tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add failing tests for equality"
```

---

## Task 12: Implement Equality

**Files:**
- Modify: `src/Tethys.Results/TypedResult.cs`

**Step 1: Replace Equals stub and add full equality implementation**

Replace the Equals stub and add these members:

```csharp
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
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "Equals_|GetHashCode_"`
Expected: PASS

**Step 3: Commit passing implementation**

```bash
git add src/Tethys.Results/TypedResult.cs
git commit -m "feat: add equality implementation"
```

---

## Task 13: Add Edge Case Tests

**Files:**
- Modify: `test/Tethys.Test/TypedResultTests.cs`

**Step 1: Add edge case tests for different type scenarios**

```csharp
[Test]
public async Task Ok_WithValueType_ShouldWork()
{
    // Arrange & Act
    var result = Result<int, int>.Ok(42);

    // Assert
    await Assert.That(result.Success).IsTrue();
    await Assert.That(result.Value).IsEqualTo(42);
}

[Test]
public async Task Fail_WithValueType_ShouldWork()
{
    // Arrange & Act
    var result = Result<int, int>.Fail(500);

    // Assert
    await Assert.That(result.Success).IsFalse();
    await Assert.That(result.Error).IsEqualTo(500);
}

[Test]
public async Task Ok_WithNullValue_ShouldWork()
{
    // Arrange & Act
    var result = Result<string, string>.Ok(null);

    // Assert
    await Assert.That(result.Success).IsTrue();
    await Assert.That(result.Value).IsNull();
}

[Test]
public async Task Fail_WithNullError_ShouldWork()
{
    // Arrange & Act
    var result = Result<string, string>.Fail(null);

    // Assert
    await Assert.That(result.Success).IsFalse();
    await Assert.That(result.Error).IsNull();
}

[Test]
public async Task Match_WithComplexTypes_ShouldWork()
{
    // Arrange
    var result = Result<DateTime, Exception>.Ok(new DateTime(2026, 1, 1));

    // Act
    var year = result.Match(
        onSuccess: date => date.Year,
        onFailure: ex => -1
    );

    // Assert
    await Assert.That(year).IsEqualTo(2026);
}
```

**Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "WithValueType|WithNull|WithComplexTypes"`
Expected: PASS

**Step 3: Commit tests**

```bash
git add test/Tethys.Test/TypedResultTests.cs
git commit -m "test: add edge case tests for value types, nulls, and complex types"
```

---

## Task 14: Run Full Test Suite and Final Commit

**Step 1: Run all tests**

Run: `dotnet test`
Expected: All 316+ tests pass (original 316 + new TypedResult tests)

**Step 2: Verify build**

Run: `dotnet build`
Expected: Build succeeded with 0 errors, 0 warnings

**Step 3: Final commit if any uncommitted changes**

```bash
git status
# If clean, no action needed
# If changes exist:
git add -A
git commit -m "chore: finalize Result<TValue, TError> implementation"
```

---

## Summary

After completing all tasks, you will have:

- **New file:** `src/Tethys.Results/TypedResult.cs` (~150 lines)
- **New file:** `test/Tethys.Test/TypedResultTests.cs` (~200 lines)
- **~20 new tests** covering:
  - Factory methods (Ok, Fail)
  - Property accessors with throwing behavior
  - Match and MatchAsync pattern matching
  - Null argument validation
  - Equality (Equals, ==, !=, GetHashCode)
  - Edge cases (value types, nulls, complex types)

The implementation follows TDD with frequent commits, matching the existing codebase style.
