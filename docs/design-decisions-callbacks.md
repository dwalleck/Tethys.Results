# Design Decisions: Callback Functionality in Tethys.Results

This document explains the design decisions, tradeoffs, and C# capabilities used in implementing the callback functionality (`OnSuccess`, `OnFailure`, `OnBoth`) for the Tethys.Results library.

## Table of Contents
- [Core Design Philosophy](#core-design-philosophy)
- [Extension Methods vs Instance Methods](#extension-methods-vs-instance-methods)
- [Generic Constraints and Overloading](#generic-constraints-and-overloading)
- [Null Safety and Validation](#null-safety-and-validation)
- [Async Implementation Details](#async-implementation-details)
- [Exception Handling Philosophy](#exception-handling-philosophy)
- [Method Naming and Discoverability](#method-naming-and-discoverability)
- [Thread Safety Through Immutability](#thread-safety-through-immutability)
- [Practical Usage Patterns](#practical-usage-patterns-enabled)
- [Key Design Tradeoffs](#key-design-tradeoffs)
- [C# Language Features Leveraged](#c-language-features-that-made-this-elegant)

## Core Design Philosophy

The callback methods follow a **fluent interface pattern** with these key principles:

```csharp
// Returns the original result for chaining
public static Result OnSuccess(this Result result, Action callback)
{
    // Execute callback conditionally
    if (result.Success) callback();
    
    // ALWAYS return the original result
    return result;
}
```

**Design Decision**: Return the original result unchanged
- **Benefit**: Enables method chaining without breaking the flow
- **Tradeoff**: Callbacks can't modify the result (immutability preserved)

## Extension Methods vs Instance Methods

**Decision**: Implemented as extension methods in `ResultExtensions.cs`

```csharp
// Extension method approach (chosen)
public static Result OnSuccess(this Result result, Action callback)

// vs. Instance method approach (not chosen)
public Result OnSuccess(Action callback) // Would be in Result.cs
```

**Benefits of Extension Methods**:
1. **Open/Closed Principle**: Can add functionality without modifying core classes
2. **Separation of Concerns**: Core Result classes stay focused on state representation
3. **Backwards Compatibility**: Can add features without breaking existing code
4. **Discoverability**: IntelliSense shows these methods on Result types

## Generic Constraints and Overloading

The design uses **method overloading** to handle different scenarios:

```csharp
// For Result (no data)
public static Result OnSuccess(this Result result, Action callback)

// For Result<T> with data access
public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> callback)

// Async versions
public static Task<Result> OnSuccessAsync(this Result result, Func<Task> callback)
public static Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<T, Task> callback)
```

**C# Capabilities Used**:
- **Generic type inference**: Don't need to specify `<T>` when calling
- **Overload resolution**: Compiler picks the right method based on types
- **Async/await support**: First-class async citizens

## Null Safety and Validation

```csharp
public static Result OnSuccess(this Result result, Action callback)
{
    if (callback == null)
        throw new ArgumentNullException(nameof(callback));
    
    // Safe to proceed
}
```

**C# Features Used**:
- `nameof()` operator for refactor-safe parameter names
- **Eager validation** before any logic execution

**Design Tradeoff**:
- **Strict**: Throws exception for null callbacks
- **Alternative**: Could silently ignore nulls
- **Decision**: Fail fast to catch programming errors early

## Async Implementation Details

```csharp
public static async Task<Result<T>> OnSuccessAsync<T>(
    this Result<T> result, 
    Func<T, Task> callback)
{
    if (callback == null)
        throw new ArgumentNullException(nameof(callback));

    if (result.Success)
    {
        await callback(result.Value).ConfigureAwait(false);
    }

    return result;
}
```

**Key C# Async Features**:
1. **ConfigureAwait(false)**: Prevents deadlocks in UI/ASP.NET contexts
2. **Task<Result<T>>**: Preserves the result type through async operations
3. **Func<T, Task>**: Allows async lambdas and method groups

## Exception Handling Philosophy

```csharp
// Callbacks can throw - we don't catch
public static Result OnSuccess(this Result result, Action callback)
{
    if (result.Success)
    {
        callback(); // If this throws, let it bubble up
    }
    return result;
}
```

**Design Decision**: Don't catch exceptions in callbacks
- **Rationale**: Callbacks are user code; they should handle their own errors
- **Benefit**: Preserves stack traces and doesn't hide bugs
- **Tradeoff**: Can break the fluent chain if callback throws

## Method Naming and Discoverability

```csharp
OnSuccess   // Clear when it executes
OnFailure   // Obvious counterpart
OnBoth      // Explicitly states it runs regardless
```

**Design Benefits**:
- **Self-documenting**: Names clearly indicate behavior
- **Consistent pattern**: On[Condition] naming scheme
- **IntelliSense friendly**: All callback methods grouped together

## Thread Safety Through Immutability

```csharp
public static Result OnSuccess(this Result result, Action callback)
{
    // Result is immutable - no synchronization needed
    if (result.Success) // Reading immutable state
    {
        callback(); // User's responsibility for thread safety
    }
    return result; // Return same instance
}
```

**Design Benefits**:
- No locks or synchronization needed
- Safe to use from multiple threads
- Callbacks execute on calling thread

## Practical Usage Patterns Enabled

The design enables elegant patterns:

```csharp
// Logging pipeline
var result = await GetUserAsync(id)
    .OnBothAsync(r => LogOperation($"GetUser: {r.Success}"))
    .ThenAsync(user => UpdateProfileAsync(user))
    .OnFailureAsync(ex => LogError(ex))
    .OnSuccessAsync(user => NotifyUserAsync(user));

// Side effects without breaking the chain
orderResult
    .OnSuccess(order => metrics.RecordSuccess())
    .OnFailure(ex => metrics.RecordFailure())
    .Match(
        order => Ok(order),
        error => BadRequest(error.Message)
    );

// Debugging and diagnostics
result
    .OnBoth(r => Console.WriteLine($"Operation completed: {r.Success}"))
    .OnFailure(ex => Debugger.Break())
    .OnSuccess(() => telemetry.TrackSuccess());
```

## Key Design Tradeoffs

### 1. Immutability vs Modification
- **Chose**: Callbacks can't modify results
- **Benefit**: Thread-safe, predictable
- **Tradeoff**: Need other methods (Map, FlatMap) for transformations

### 2. Extension Methods vs Inheritance
- **Chose**: Extension methods
- **Benefit**: Non-invasive, backwards compatible
- **Tradeoff**: Can't access private members

### 3. Exception Propagation
- **Chose**: Let callback exceptions bubble up
- **Benefit**: Don't hide errors
- **Tradeoff**: Can break fluent chains

### 4. Async All The Way
- **Chose**: Separate async methods for all callbacks
- **Benefit**: First-class async support
- **Tradeoff**: More API surface area

### 5. Return Type Consistency
- **Chose**: Always return the original result
- **Benefit**: Predictable chaining behavior
- **Tradeoff**: Can't short-circuit chains

## C# Language Features That Made This Elegant

### 1. Extension Methods
```csharp
public static Result OnSuccess(this Result result, Action callback)
```
Add methods to existing types without modification.

### 2. Generic Type Inference
```csharp
result.OnSuccess(value => Console.WriteLine(value)); // T inferred
```
Clean syntax without explicit type parameters.

### 3. Expression-bodied Members
```csharp
public static Result OnBoth(this Result result, Action<Result> callback)
{
    callback(result);
    return result;
}
```
Concise implementations for simple methods.

### 4. Async/Await
```csharp
await result.OnSuccessAsync(async value => 
{
    await ProcessAsync(value);
});
```
Natural async programming model.

### 5. Method Overloading
Same name, different signatures for different scenarios.

### 6. Action/Func Delegates
First-class functions without custom delegate types.

### 7. ConfigureAwait
```csharp
await callback(result.Value).ConfigureAwait(false);
```
Fine-grained async context control.

### 8. nameof Operator
```csharp
throw new ArgumentNullException(nameof(callback));
```
Refactoring-safe parameter names.

### 9. XML Documentation
```csharp
/// <summary>
/// Executes the specified callback if the result is successful.
/// </summary>
```
IntelliSense integration for better developer experience.

## Conclusion

The callback design prioritizes **composability**, **safety**, and **developer experience** while maintaining the immutable, functional nature of the Result pattern. By leveraging modern C# features and following established patterns, we've created an API that is both powerful and intuitive to use.

The key insight is that callbacks are about **side effects**, not transformations. For transformations, users should use `Map` or `FlatMap`. This separation of concerns leads to cleaner, more maintainable code.