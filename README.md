# PhoenixLabs.Results

A lightweight, thread-safe Result pattern implementation for .NET that provides a clean way to handle operation results with success/failure states, error messages, and optional data.

## Features

- Simple and intuitive API for handling operation results
- Generic and non-generic result types
- Thread-safe and immutable implementation
- Comprehensive error handling with exception support
- Fluent extension methods for chaining operations
- Implicit conversion operators for seamless integration
- Aggregate error handling for combining multiple results
- Full XML documentation

## Installation

Install the package via NuGet:

```bash
dotnet add package PhoenixLabs.Results
```

Or via the NuGet Package Manager:

```
Install-Package PhoenixLabs.Results
```

## Usage

### Basic Usage

```csharp
using PhoenixLabs.Results;

// Create a successful result
var successResult = Result.Ok("Operation completed successfully");

// Create a failed result
var failResult = Result.Fail("Something went wrong");

// Create a failed result with an exception
var exceptionResult = Result.FromException(new Exception("An error occurred"));

// Check if the operation was successful
if (successResult.Success)
{
    Console.WriteLine(successResult.Message);
}
```

### Working with Data

```csharp
// Create a successful result with data
var dataResult = Result<int>.Ok(42);

// Create a successful result with data and a custom message
var customDataResult = Result<string>.Ok("Hello, world!", "String retrieved successfully");

// Access the data
if (dataResult.Success)
{
    Console.WriteLine($"The answer is: {dataResult.Data}");
}

// Get value or default
int value = dataResult.GetValueOrDefault();

// Try get value
if (dataResult.TryGetValue(out int result))
{
    Console.WriteLine($"Got value: {result}");
}

// Get value or throw
try
{
    int mustHaveValue = dataResult.GetValueOrThrow();
    Console.WriteLine($"Value: {mustHaveValue}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Implicit conversion (will throw if result is not successful)
int answer = dataResult;
```

### Combining Results

```csharp
// Combine multiple results
var results = new List<Result>
{
    Result.Ok("First operation succeeded"),
    Result.Fail("Second operation failed"),
    Result.FromException(new Exception("Third operation threw an exception"))
};

var combinedResult = Result.Combine(results);

// Combine multiple results with data
var dataResults = new List<Result<int>>
{
    Result<int>.Ok(1),
    Result<int>.Ok(2),
    Result<int>.Fail("Failed to get third value")
};

var combinedDataResult = Result<int>.Combine(dataResults);
if (combinedDataResult.Success)
{
    foreach (var item in combinedDataResult.Data)
    {
        Console.WriteLine(item);
    }
}
```

## Continuous Integration

This project uses GitHub Actions for continuous integration and deployment:

- **Build and Test**: Automatically builds and tests the project on every push to the main branch and pull requests.
- **Publish NuGet Package**: Automatically publishes the package to NuGet.org when a new release is created on GitHub.

To use these workflows:

1. Fork or clone this repository
2. For publishing to NuGet.org, add your NuGet API key as a GitHub secret named `NUGET_API_KEY`
3. Create a new release on GitHub to trigger the publish workflow

## License

This project is licensed under the MIT License - see the LICENSE file for details.
