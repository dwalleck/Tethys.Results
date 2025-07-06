# Getting Started with Tethys.Results

Welcome to Tethys.Results! This guide will help you get up and running with the Result pattern for clean, functional error handling in your .NET applications.

## Table of Contents
- [Installation](#installation)
- [Basic Concepts](#basic-concepts)
- [Your First Result](#your-first-result)
- [Working with Values](#working-with-values)
- [Error Handling](#error-handling)
- [Chaining Operations](#chaining-operations)
- [Async Operations](#async-operations)
- [Next Steps](#next-steps)

## Installation

Install Tethys.Results via NuGet Package Manager:

```bash
dotnet add package Tethys.Results
```

Or via Package Manager Console:

```powershell
Install-Package Tethys.Results
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="Tethys.Results" Version="*" />
```

## Basic Concepts

The Result pattern provides a way to handle operations that can succeed or fail without throwing exceptions. This approach offers several benefits:

- **Explicit error handling** - Errors are part of the method signature
- **Composable operations** - Chain multiple operations together
- **Thread-safe** - Immutable design ensures thread safety
- **Better performance** - Avoid the overhead of exceptions for expected failures

### Key Types

1. **`Result`** - Represents an operation that can succeed or fail
2. **`Result<T>`** - Represents an operation that returns a value on success
3. **`AggregateError`** - Represents multiple errors aggregated together

## Your First Result

Let's start with simple success and failure cases:

```csharp
using Tethys.Results;

// Creating a successful result
Result successResult = Result.Ok("Operation completed successfully");
Console.WriteLine($"Success: {successResult.Success}"); // True
Console.WriteLine($"Message: {successResult.Message}"); // "Operation completed successfully"

// Creating a failed result
Result failureResult = Result.Fail("Something went wrong");
Console.WriteLine($"Success: {failureResult.Success}"); // False
Console.WriteLine($"Message: {failureResult.Message}"); // "Something went wrong"
```

## Working with Values

When your operation returns data, use `Result<T>`:

```csharp
// Success with data
Result<int> calculateResult = Result<int>.Ok(42, "Calculation completed");
if (calculateResult.Success)
{
    int value = calculateResult.Data;
    Console.WriteLine($"The answer is: {value}");
}

// Failure without data
Result<string> fetchResult = Result<string>.Fail("Network error occurred");
if (!fetchResult.Success)
{
    Console.WriteLine($"Failed to fetch data: {fetchResult.Message}");
}
```

### Extracting Values Safely

There are several ways to extract values from a `Result<T>`:

```csharp
Result<string> userResult = GetUserName(userId);

// Method 1: Check Success property
if (userResult.Success)
{
    string userName = userResult.Data;
    Console.WriteLine($"User: {userName}");
}

// Method 2: Try pattern
if (userResult.TryGetValue(out string name))
{
    Console.WriteLine($"User: {name}");
}

// Method 3: Get value or default
string displayName = userResult.GetValueOrDefault("Guest");
Console.WriteLine($"Welcome, {displayName}!");

// Method 4: Get value or throw (use sparingly)
try
{
    string userName = userResult.GetValueOrThrow();
    Console.WriteLine($"User: {userName}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Error Handling

Results can include exception information for detailed error tracking:

```csharp
try
{
    // Some operation that might throw
    int result = int.Parse("not a number");
    return Result<int>.Ok(result);
}
catch (FormatException ex)
{
    // Capture the exception with the result
    return Result<int>.Fail("Invalid number format", ex);
}
```

You can also create results directly from exceptions:

```csharp
try
{
    // Risky operation
    var data = FetchDataFromApi();
    return Result<string>.Ok(data);
}
catch (Exception ex)
{
    // Create result from exception
    return Result<string>.Fail(ex);
}
```

## Chaining Operations

One of the most powerful features is the ability to chain operations:

```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    return ValidateRequest(request)
        .Then(() => CreateOrder(request))
        .Then(order => ApplyDiscount(order))
        .Then(order => CalculateShipping(order))
        .Then(order => Result<Order>.Ok(order, "Order processed successfully"));
}

// Each method returns a Result or Result<T>
private Result ValidateRequest(OrderRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerEmail))
        return Result.Fail("Customer email is required");
    
    if (request.Items.Count == 0)
        return Result.Fail("Order must contain at least one item");
    
    return Result.Ok("Validation passed");
}

private Result<Order> CreateOrder(OrderRequest request)
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerEmail = request.CustomerEmail,
        Items = request.Items,
        CreatedAt = DateTime.UtcNow
    };
    
    return Result<Order>.Ok(order, "Order created");
}

private Result<Order> ApplyDiscount(Order order)
{
    if (order.Items.Count >= 5)
    {
        order.Discount = 0.1m; // 10% discount
        return Result<Order>.Ok(order, "Discount applied");
    }
    
    return Result<Order>.Ok(order, "No discount applicable");
}

private Result<Order> CalculateShipping(Order order)
{
    order.ShippingCost = order.Items.Count * 2.50m;
    return Result<Order>.Ok(order, "Shipping calculated");
}
```

### Conditional Execution

Use `When` to conditionally execute operations:

```csharp
var result = GetUserSettings(userId)
    .When(settings => settings.EmailNotifications, 
          settings => SendNotificationEmail(settings.Email))
    .When(settings => settings.SmsNotifications,
          settings => SendSms(settings.PhoneNumber));
```

## Async Operations

Tethys.Results provides full support for async operations:

```csharp
public async Task<Result<WeatherData>> GetWeatherAsync(string city)
{
    return await ValidateCityName(city)
        .ThenAsync(async validCity => await FetchWeatherDataAsync(validCity))
        .ThenAsync(async data => await EnrichWithForecastAsync(data))
        .ThenAsync(async enrichedData =>
        {
            await CacheWeatherDataAsync(enrichedData);
            return Result<WeatherData>.Ok(enrichedData, "Weather data retrieved");
        });
}

private Result<string> ValidateCityName(string city)
{
    if (string.IsNullOrWhiteSpace(city))
        return Result<string>.Fail("City name cannot be empty");
    
    return Result<string>.Ok(city.Trim());
}

private async Task<Result<WeatherData>> FetchWeatherDataAsync(string city)
{
    try
    {
        var response = await _httpClient.GetAsync($"/weather?city={city}");
        if (!response.IsSuccessStatusCode)
            return Result<WeatherData>.Fail($"Failed to fetch weather for {city}");
        
        var data = await response.Content.ReadFromJsonAsync<WeatherData>();
        return Result<WeatherData>.Ok(data, "Weather data fetched");
    }
    catch (Exception ex)
    {
        return Result<WeatherData>.Fail($"Error fetching weather: {ex.Message}", ex);
    }
}
```

### Working with Task<Result>

You can also chain operations on `Task<Result>`:

```csharp
Task<Result<int>> asyncOperation = GetNumberAsync();

var finalResult = await asyncOperation
    .ThenAsync(async number =>
    {
        await Task.Delay(100); // Simulate work
        return Result<string>.Ok($"Number is: {number}");
    });
```

## Next Steps

Now that you understand the basics, explore these advanced topics:

1. **[API Reference](api-reference.md)** - Complete API documentation
2. **[Examples](examples.md)** - Common usage patterns and real-world scenarios
3. **Error Aggregation** - Combine multiple results and handle aggregate errors
4. **Integration Patterns** - Using Results with ASP.NET Core, Entity Framework, etc.
5. **Best Practices** - Guidelines for effective Result pattern usage

### Quick Tips

1. **Prefer Results over Exceptions** for expected failures (validation, business rules)
2. **Use Exceptions** for truly exceptional cases (out of memory, etc.)
3. **Chain operations** instead of nested if statements
4. **Be explicit** with error messages to help debugging
5. **Leverage implicit conversions** to simplify code where appropriate

### Example: Putting It All Together

Here's a complete example showing various features:

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    
    public UserService(IUserRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
    
    public async Task<Result<User>> RegisterUserAsync(RegisterRequest request)
    {
        // Validate input
        var validationResult = ValidateRegistration(request);
        if (!validationResult.Success)
            return Result<User>.Fail(validationResult.Message);
        
        // Check if user exists
        var existingUser = await _repository.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<User>.Fail("A user with this email already exists");
        
        // Create and save user
        return await CreateUser(request)
            .ThenAsync(async user => await _repository.SaveAsync(user))
            .ThenAsync(async user => await SendWelcomeEmail(user))
            .ThenAsync(async user =>
            {
                return Result<User>.Ok(user, "User registered successfully");
            });
    }
    
    private Result ValidateRegistration(RegisterRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(request.Email))
            errors.Add("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");
        else if (request.Password.Length < 8)
            errors.Add("Password must be at least 8 characters");
        
        if (errors.Any())
            return Result.Fail(string.Join("; ", errors));
        
        return Result.Ok("Validation passed");
    }
    
    private Result<User> CreateUser(RegisterRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        
        return Result<User>.Ok(user);
    }
    
    private async Task<Result<User>> SendWelcomeEmail(User user)
    {
        var emailResult = await _emailService.SendWelcomeEmailAsync(user.Email);
        
        return emailResult.Success
            ? Result<User>.Ok(user, "Welcome email sent")
            : Result<User>.Ok(user, "User created but welcome email failed");
    }
    
    private bool IsValidEmail(string email)
    {
        // Simple email validation
        return email.Contains("@") && email.Contains(".");
    }
    
    private string HashPassword(string password)
    {
        // Simplified - use proper hashing in production
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }
}
```

Happy coding with Tethys.Results!