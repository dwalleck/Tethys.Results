# API Reference

This document provides a comprehensive reference for all public APIs in Tethys.Results.

## Table of Contents
- [Result Class](#result-class)
- [Result<T> Class](#resultt-class)
- [Extension Methods](#extension-methods)
- [AggregateError Class](#aggregateerror-class)
- [Thread Safety](#thread-safety)

## Result Class

The `Result` class represents the outcome of an operation that can succeed or fail without a return value.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Success` | `bool` | Indicates whether the operation succeeded |
| `Message` | `string` | A descriptive message about the operation result |
| `Exception` | `Exception` | The exception associated with a failed result (null for success) |

### Static Methods

#### `Result.Ok()`
Creates a successful result with a default success message.

```csharp
Result result = Result.Ok();
// result.Success = true
// result.Message = "Operation completed successfully"
```

#### `Result.Ok(string message)`
Creates a successful result with a custom message.

```csharp
Result result = Result.Ok("User created successfully");
// result.Success = true
// result.Message = "User created successfully"
```

#### `Result.Fail(string message)`
Creates a failed result with an error message.

```csharp
Result result = Result.Fail("Invalid username");
// result.Success = false
// result.Message = "Invalid username"
// result.Exception = null
```

#### `Result.Fail(string message, Exception exception)`
Creates a failed result with an error message and associated exception.

```csharp
var ex = new ArgumentException("Invalid argument");
Result result = Result.Fail("Validation failed", ex);
// result.Success = false
// result.Message = "Validation failed"
// result.Exception = ex
```

#### `Result.Fail(Exception exception)`
Creates a failed result from an exception, using the exception's message.

```csharp
var ex = new InvalidOperationException("Operation not allowed");
Result result = Result.Fail(ex);
// result.Success = false
// result.Message = "Operation not allowed"
// result.Exception = ex
```

#### `Result.Combine(IEnumerable<Result> results)`
Combines multiple results into a single result. If all results are successful, returns success. If any fail, returns failure with aggregated errors.

```csharp
var results = new List<Result>
{
    Result.Ok("Step 1 completed"),
    Result.Ok("Step 2 completed"),
    Result.Fail("Step 3 failed")
};

Result combined = Result.Combine(results);
// combined.Success = false
// combined.Message = "One or more operations failed"
// combined.Exception is AggregateError with details
```

**Exceptions:**
- `ArgumentNullException` - if `results` is null
- `ArgumentException` - if `results` is empty

## Result<T> Class

The `Result<T>` class represents the outcome of an operation that returns a value of type `T` on success.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Success` | `bool` | Indicates whether the operation succeeded |
| `Message` | `string` | A descriptive message about the operation result |
| `Exception` | `Exception` | The exception associated with a failed result (null for success) |
| `Data` | `T` | The value returned by a successful operation (default(T) for failure) |
| `Value` | `T` | Alias for `Data` property |

### Static Methods

#### `Result<T>.Ok(T value)`
Creates a successful result with a value and default message.

```csharp
Result<int> result = Result<int>.Ok(42);
// result.Success = true
// result.Data = 42
// result.Message = "Operation completed successfully"
```

#### `Result<T>.Ok(T value, string message)`
Creates a successful result with a value and custom message.

```csharp
Result<string> result = Result<string>.Ok("John Doe", "User found");
// result.Success = true
// result.Data = "John Doe"
// result.Message = "User found"
```

#### `Result<T>.Fail(string message)`
Creates a failed result with an error message.

```csharp
Result<int> result = Result<int>.Fail("Division by zero");
// result.Success = false
// result.Message = "Division by zero"
// result.Data = 0 (default for int)
```

#### `Result<T>.Fail(string message, Exception exception)`
Creates a failed result with an error message and exception.

```csharp
var ex = new FileNotFoundException("Config file not found");
Result<Config> result = Result<Config>.Fail("Failed to load config", ex);
// result.Success = false
// result.Message = "Failed to load config"
// result.Exception = ex
// result.Data = null
```

#### `Result<T>.Fail(Exception exception)`
Creates a failed result from an exception.

```csharp
var ex = new HttpRequestException("Network error");
Result<string> result = Result<string>.Fail(ex);
// result.Success = false
// result.Message = "Network error"
// result.Exception = ex
```

#### `Result<T>.Combine(IEnumerable<Result<T>> results)`
Combines multiple results with values. If all succeed, returns success with all values. If any fail, returns failure with aggregated errors.

```csharp
var results = new List<Result<int>>
{
    Result<int>.Ok(1),
    Result<int>.Ok(2),
    Result<int>.Ok(3)
};

Result<int> combined = Result<int>.Combine(results);
// combined.Success = true
// combined.Data is IEnumerable<int> { 1, 2, 3 }
```

**Exceptions:**
- `ArgumentNullException` - if `results` is null
- `ArgumentException` - if `results` is empty

### Implicit Conversions

#### From Value to Result<T>
```csharp
Result<string> result = "Hello"; // Implicitly converts to Result<string>.Ok("Hello")
Result<int> number = 42; // Implicitly converts to Result<int>.Ok(42)
```

#### From Result<T> to Value
```csharp
Result<int> result = Result<int>.Ok(42);
int value = result; // Implicitly converts to 42

// Warning: Throws InvalidOperationException if result.Success is false
Result<int> failed = Result<int>.Fail("Error");
int bad = failed; // Throws!
```

## Extension Methods

Extension methods provide fluent chaining capabilities for Result operations.

### Result Extensions

#### `Then(Func<Result> operation)`
Executes the next operation if the current result is successful.

```csharp
Result result = Result.Ok()
    .Then(() => ValidateData())
    .Then(() => SaveToDatabase());
```

#### `Then<T>(Func<Result<T>> operation)`
Executes the next operation that returns a value if the current result is successful.

```csharp
Result<User> result = Result.Ok()
    .Then(() => Result<User>.Ok(new User { Name = "John" }));
```

#### `ThenAsync(Func<Task<Result>> operation)`
Asynchronously executes the next operation if the current result is successful.

```csharp
Result result = await Result.Ok()
    .ThenAsync(async () =>
    {
        await Task.Delay(100);
        return Result.Ok("Async operation completed");
    });
```

#### `ThenAsync<T>(Func<Task<Result<T>>> operation)`
Asynchronously executes the next operation that returns a value.

```csharp
Result<string> result = await Result.Ok()
    .ThenAsync(async () =>
    {
        await Task.Delay(100);
        return Result<string>.Ok("Async value");
    });
```

#### `When(bool condition, Func<Result> operation)`
Conditionally executes an operation based on a boolean condition.

```csharp
Result result = Result.Ok()
    .When(userIsAdmin, () => GrantAdminPrivileges())
    .When(needsNotification, () => SendNotification());
```

### Result<T> Extensions

#### `Then(Func<T, Result> operation)`
Executes the next operation with the current value if successful.

```csharp
Result result = Result<int>.Ok(5)
    .Then(value => value > 0 
        ? Result.Ok($"Value {value} is positive") 
        : Result.Fail("Value must be positive"));
```

#### `Then<TNext>(Func<T, Result<TNext>> operation)`
Transforms the value through an operation if successful.

```csharp
Result<string> result = Result<int>.Ok(42)
    .Then(value => Result<string>.Ok(value.ToString()));
```

#### `ThenAsync(Func<T, Task<Result>> operation)`
Asynchronously executes an operation with the current value.

```csharp
Result result = await Result<string>.Ok("user@example.com")
    .ThenAsync(async email =>
    {
        await SendEmailAsync(email);
        return Result.Ok("Email sent");
    });
```

#### `ThenAsync<TNext>(Func<T, Task<Result<TNext>>> operation)`
Asynchronously transforms the value.

```csharp
Result<User> result = await Result<int>.Ok(123)
    .ThenAsync(async id =>
    {
        var user = await GetUserAsync(id);
        return Result<User>.Ok(user);
    });
```

#### `When(bool condition, Func<Result<T>> operation)`
Conditionally executes an operation, maintaining the value type.

```csharp
Result<int> result = Result<int>.Ok(10)
    .When(true, () => Result<int>.Ok(20)); // Returns 20
```

#### `GetValueOrDefault(T defaultValue = default)`
Gets the value or returns a default if the result is failed.

```csharp
Result<string> success = Result<string>.Ok("Hello");
string value1 = success.GetValueOrDefault("Default"); // "Hello"

Result<string> failure = Result<string>.Fail("Error");
string value2 = failure.GetValueOrDefault("Default"); // "Default"
```

#### `TryGetValue(out T value)`
Attempts to get the value using the Try pattern.

```csharp
Result<int> result = Result<int>.Ok(42);

if (result.TryGetValue(out int value))
{
    Console.WriteLine($"Got value: {value}"); // Prints: Got value: 42
}
else
{
    Console.WriteLine("Failed to get value");
}
```

#### `GetValueOrThrow()`
Gets the value or throws an exception if failed.

```csharp
Result<string> success = Result<string>.Ok("Success");
string value = success.GetValueOrThrow(); // Returns "Success"

Result<string> failure = Result<string>.Fail("Error occurred");
string bad = failure.GetValueOrThrow(); // Throws InvalidOperationException
```

**Exceptions:**
- `InvalidOperationException` - if the result is failed

### Task<Result> Extensions

Extensions for working with async results.

#### `ThenAsync(Func<Task<Result>> operation)`
Chains async operations on Task<Result>.

```csharp
Task<Result> task = GetResultAsync();
Result finalResult = await task.ThenAsync(async () =>
{
    await DoMoreWorkAsync();
    return Result.Ok();
});
```

#### `ThenAsync<T>(Func<Task<Result<T>>> operation)`
Chains async operations that return values.

```csharp
Task<Result> task = GetResultAsync();
Result<string> finalResult = await task.ThenAsync(async () =>
{
    var data = await FetchDataAsync();
    return Result<string>.Ok(data);
});
```

### Task<Result<T>> Extensions

#### `ThenAsync(Func<T, Task<Result>> operation)`
Chains async operations with the value from Task<Result<T>>.

```csharp
Task<Result<int>> task = GetNumberAsync();
Result finalResult = await task.ThenAsync(async value =>
{
    await ProcessNumberAsync(value);
    return Result.Ok();
});
```

#### `ThenAsync<TNext>(Func<T, Task<Result<TNext>>> operation)`
Transforms values asynchronously.

```csharp
Task<Result<int>> task = GetNumberAsync();
Result<string> finalResult = await task.ThenAsync(async value =>
{
    var processed = await ProcessAsync(value);
    return Result<string>.Ok(processed);
});
```

## AggregateError Class

Represents multiple errors aggregated from combined operations.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ErrorMessages` | `List<string>` | Collection of error messages |
| `InnerErrors` | `List<Exception>` | Collection of inner exceptions |
| `Message` | `string` | Combined error message |

### Constructors

#### `AggregateError(IEnumerable<string> messages)`
Creates an aggregate error from error messages.

```csharp
var messages = new[] { "Error 1", "Error 2", "Error 3" };
var error = new AggregateError(messages);
// error.Message = "Multiple errors occurred: Error 1; Error 2; Error 3"
```

#### `AggregateError(IEnumerable<Exception> exceptions)`
Creates an aggregate error from exceptions.

```csharp
var exceptions = new[]
{
    new ArgumentException("Invalid arg"),
    new InvalidOperationException("Invalid op")
};
var error = new AggregateError(exceptions);
// error.ErrorMessages = ["Invalid arg", "Invalid op"]
// error.InnerErrors = exceptions
```

## Thread Safety

All Result types are immutable and thread-safe. Once created, a Result instance cannot be modified, making it safe to:

- Share across multiple threads
- Use in concurrent operations
- Store in shared caches
- Pass between async operations

### Example: Thread-Safe Usage

```csharp
// This result can be safely shared across threads
private static readonly Result<Config> _configResult = LoadConfiguration();

public async Task ProcessInParallel(List<Item> items)
{
    // Safe to use the same result instance in parallel operations
    await Parallel.ForEachAsync(items, async (item, ct) =>
    {
        if (_configResult.Success)
        {
            await ProcessItem(item, _configResult.Data);
        }
    });
}
```

### Immutability Guarantees

1. All properties are read-only
2. No methods modify the instance
3. Collections (like in AggregateError) are defensive copies
4. Extension methods return new instances

```csharp
var result1 = Result.Ok("Step 1");
var result2 = result1.Then(() => Result.Ok("Step 2"));

// result1 is unchanged - still "Step 1"
// result2 is a new instance with "Step 2"
```