# Tethys.Results

[![NuGet](https://img.shields.io/nuget/v/Tethys.Results.svg)](https://www.nuget.org/packages/Tethys.Results/)
[![Build Status](https://github.com/dwalleck/Tethys.Results/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/dwalleck/Tethys.Results/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, thread-safe Result pattern implementation for .NET that provides a clean, functional approach to error handling without exceptions.

## Features

- ✅ **Simple and Intuitive API** - Easy to understand and use
- ✅ **Thread-Safe** - Immutable design ensures thread safety
- ✅ **No Dependencies** - Lightweight with zero external dependencies
- ✅ **Async Support** - First-class support for async/await patterns
- ✅ **Functional Composition** - Chain operations with `Then` and `When`
- ✅ **Type-Safe** - Generic `Result<T>` for operations that return values
- ✅ **Error Aggregation** - Combine multiple results and aggregate errors
- ✅ **Implicit Conversions** - Seamless conversion between values and Results

## Installation

```bash
dotnet add package Tethys.Results
```

## Quick Start

### Basic Usage

```csharp
using Tethys.Results;

// Simple success/failure results
Result successResult = Result.Ok();
Result failureResult = Result.Fail("Something went wrong");

// Results with values
Result<int> valueResult = Result<int>.Ok(42);
Result<string> errorResult = Result<string>.Fail("Not found");

// Using implicit conversions
Result<int> implicitSuccess = 42;
Result<string> implicitError = new Exception("Error occurred");
```

### Chaining Operations

```csharp
var result = GetUser(userId)
    .Then(user => UpdateProfile(user, newData))
    .Then(user => SendNotification(user))
    .Then(user => LogActivity(user));

if (result.Success)
{
    Console.WriteLine("All operations completed successfully");
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

### Async Support

```csharp
var result = await GetUserAsync(userId)
    .ThenAsync(user => UpdateProfileAsync(user, newData))
    .ThenAsync(user => SendNotificationAsync(user))
    .ThenAsync(user => LogActivityAsync(user));
```

### Conditional Execution

```csharp
var result = GetConfiguration()
    .When(config => config.IsEnabled, config => ProcessData(config))
    .Then(data => SaveResults(data));
```

### Error Aggregation

```csharp
var results = new[]
{
    ValidateEmail(email),
    ValidatePassword(password),
    ValidateUsername(username)
};

var combined = Result.Combine(results);
if (!combined.Success)
{
    // All error messages are aggregated
    Console.WriteLine($"Validation failed: {combined.Message}");
}
```

### Value Extraction

```csharp
Result<User> userResult = GetUser(userId);

// Get value or default
User user = userResult.GetValueOrDefault(new User { Name = "Guest" });

// Try pattern
if (userResult.TryGetValue(out User user))
{
    Console.WriteLine($"Found user: {user.Name}");
}

// Get value or throw
try
{
    User user = userResult.GetValueOrThrow();
}
catch (InvalidOperationException ex)
{
    // Handle the exception
}
```

## API Reference

### Result Class

- `Result.Ok()` - Creates a successful result
- `Result.Ok(string message)` - Creates a successful result with a message
- `Result.Fail(string message)` - Creates a failed result with an error message
- `Result.Fail(Exception exception)` - Creates a failed result from an exception
- `Result.Combine(IEnumerable<Result> results)` - Combines multiple results

### Result<T> Class

- `Result<T>.Ok(T value)` - Creates a successful result with a value
- `Result<T>.Ok(T value, string message)` - Creates a successful result with a value and message
- `Result<T>.Fail(string message)` - Creates a failed result with an error message
- `Result<T>.Fail(Exception exception)` - Creates a failed result from an exception
- `Result<T>.Combine(IEnumerable<Result<T>> results)` - Combines multiple results with values

### Extension Methods

- `Then()` - Chains operations on successful results
- `ThenAsync()` - Chains async operations on successful results
- `When()` - Conditionally executes operations
- `GetValueOrDefault()` - Gets the value or a default
- `TryGetValue()` - Tries to get the value using the Try pattern
- `GetValueOrThrow()` - Gets the value or throws an exception

## Advanced Usage

### Custom Error Handling

```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    return ValidateOrder(request)
        .Then(validRequest => CreateOrder(validRequest))
        .Then(order => ChargePayment(order))
        .Then(order => SendConfirmation(order))
        .OnFailure(error => LogError(error));  // Coming in v1.1.0
}
```

### Integration with ASP.NET Core

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    
    return result.Success
        ? Ok(result.Value)
        : NotFound(result.Message);
}
```

## Best Practices

1. **Use Result for Expected Failures** - Reserve exceptions for truly exceptional cases
2. **Chain Operations** - Leverage `Then` for clean, readable code
3. **Avoid Nested Results** - Use `Then` instead of manual checking
4. **Consistent Error Messages** - Provide clear, actionable error messages
5. **Leverage Implicit Conversions** - Simplify code with implicit conversions

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📧 Email: support@tethys.dev
- 🐛 Issues: [GitHub Issues](https://github.com/dwalleck/Tethys.Results/issues)
- 📖 Documentation: [Full Documentation](https://github.com/dwalleck/Tethys.Results/wiki)

## Acknowledgments

Inspired by functional programming patterns and the Railway Oriented Programming approach.