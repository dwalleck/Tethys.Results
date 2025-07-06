# Examples and Common Usage Patterns

This guide demonstrates common usage patterns and real-world scenarios for Tethys.Results, with examples derived from actual test cases.

## Table of Contents
- [Basic Operations](#basic-operations)
- [Error Handling Patterns](#error-handling-patterns)
- [Chaining Operations](#chaining-operations)
- [Async Patterns](#async-patterns)
- [Value Extraction](#value-extraction)
- [Error Aggregation](#error-aggregation)
- [Implicit Conversions](#implicit-conversions)
- [Real-World Scenarios](#real-world-scenarios)
- [Integration Patterns](#integration-patterns)
- [Best Practices](#best-practices)

## Basic Operations

### Creating Results

```csharp
// Simple success
var success = Result.Ok("Operation completed");
Console.WriteLine($"Success: {success.Success}"); // True
Console.WriteLine($"Message: {success.Message}"); // "Operation completed"

// Simple failure
var failure = Result.Fail("Something went wrong");
Console.WriteLine($"Success: {failure.Success}"); // False
Console.WriteLine($"Message: {failure.Message}"); // "Something went wrong"

// Success with data
var dataResult = Result<string>.Ok("Hello World", "Data retrieved");
Console.WriteLine($"Success: {dataResult.Success}"); // True
Console.WriteLine($"Data: {dataResult.Data}"); // "Hello World"
Console.WriteLine($"Message: {dataResult.Message}"); // "Data retrieved"

// Failure with exception
var exception = new ArgumentException("Invalid input");
var errorResult = Result.Fail("Validation failed", exception);
Console.WriteLine($"Success: {errorResult.Success}"); // False
Console.WriteLine($"Exception: {errorResult.Exception?.GetType().Name}"); // "ArgumentException"
```

### Working with Exceptions

```csharp
// Create result from exception
try
{
    // Some operation that throws
    throw new InvalidOperationException("Cannot perform this operation");
}
catch (Exception ex)
{
    var result = Result.Fail(ex);
    Console.WriteLine($"Message: {result.Message}"); // "Cannot perform this operation"
    Console.WriteLine($"Has Exception: {result.Exception != null}"); // True
}

// Wrap operations that might throw
public Result<int> SafeDivide(int a, int b)
{
    try
    {
        if (b == 0)
            return Result<int>.Fail("Cannot divide by zero");
        
        return Result<int>.Ok(a / b, "Division successful");
    }
    catch (Exception ex)
    {
        return Result<int>.Fail("Unexpected error during division", ex);
    }
}
```

## Error Handling Patterns

### Validation Pattern

```csharp
public class UserValidator
{
    public Result ValidateUser(User user)
    {
        if (user == null)
            return Result.Fail("User cannot be null");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            return Result.Fail("Email is required");
        
        if (!IsValidEmail(user.Email))
            return Result.Fail("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(user.Name))
            return Result.Fail("Name is required");
        
        if (user.Age < 18)
            return Result.Fail("User must be at least 18 years old");
        
        return Result.Ok("User validation passed");
    }
    
    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}

// Usage
var validator = new UserValidator();
var user = new User { Email = "john@example.com", Name = "John Doe", Age = 25 };
var result = validator.ValidateUser(user);

if (result.Success)
{
    Console.WriteLine("User is valid!");
}
else
{
    Console.WriteLine($"Validation failed: {result.Message}");
}
```

### Repository Pattern

```csharp
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(int id);
    Task<Result<User>> CreateAsync(User user);
    Task<Result> UpdateAsync(User user);
    Task<Result> DeleteAsync(int id);
}

public class UserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    
    public async Task<Result<User>> GetByIdAsync(int id)
    {
        await Task.Delay(10); // Simulate async work
        
        if (_users.TryGetValue(id, out var user))
        {
            return Result<User>.Ok(user, $"User {id} found");
        }
        
        return Result<User>.Fail($"User with ID {id} not found");
    }
    
    public async Task<Result<User>> CreateAsync(User user)
    {
        await Task.Delay(10); // Simulate async work
        
        if (_users.ContainsKey(user.Id))
        {
            return Result<User>.Fail($"User with ID {user.Id} already exists");
        }
        
        _users[user.Id] = user;
        return Result<User>.Ok(user, "User created successfully");
    }
    
    public async Task<Result> UpdateAsync(User user)
    {
        await Task.Delay(10); // Simulate async work
        
        if (!_users.ContainsKey(user.Id))
        {
            return Result.Fail($"User with ID {user.Id} not found");
        }
        
        _users[user.Id] = user;
        return Result.Ok("User updated successfully");
    }
    
    public async Task<Result> DeleteAsync(int id)
    {
        await Task.Delay(10); // Simulate async work
        
        if (_users.Remove(id))
        {
            return Result.Ok($"User {id} deleted successfully");
        }
        
        return Result.Fail($"User with ID {id} not found");
    }
}
```

## Chaining Operations

### Sequential Operations

```csharp
public Result<Order> ProcessOrder(OrderRequest request)
{
    // Chain multiple operations - stops at first failure
    return ValidateOrderRequest(request)
        .Then(() => CheckInventory(request.Items))
        .Then(() => CreateOrder(request))
        .Then(order => CalculatePricing(order))
        .Then(order => ApplyDiscounts(order))
        .Then(order => Result<Order>.Ok(order, "Order processed successfully"));
}

// Example implementation
private Result ValidateOrderRequest(OrderRequest request)
{
    if (request.Items == null || !request.Items.Any())
        return Result.Fail("Order must contain at least one item");
    
    if (string.IsNullOrEmpty(request.CustomerEmail))
        return Result.Fail("Customer email is required");
    
    return Result.Ok("Order request is valid");
}

private Result CheckInventory(List<OrderItem> items)
{
    foreach (var item in items)
    {
        if (GetStockLevel(item.ProductId) < item.Quantity)
            return Result.Fail($"Insufficient stock for product {item.ProductId}");
    }
    
    return Result.Ok("All items are in stock");
}

private Result<Order> CreateOrder(OrderRequest request)
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerEmail = request.CustomerEmail,
        Items = request.Items,
        CreatedAt = DateTime.UtcNow,
        Status = OrderStatus.Pending
    };
    
    return Result<Order>.Ok(order, "Order created");
}
```

### Conditional Execution

```csharp
public Result<User> UpdateUserProfile(int userId, UpdateProfileRequest request)
{
    return GetUser(userId)
        .When(request.UpdateEmail, user => UpdateEmail(user, request.Email))
        .When(request.UpdatePhone, user => UpdatePhone(user, request.Phone))
        .When(request.UpdateAddress, user => UpdateAddress(user, request.Address))
        .Then(user => SaveUser(user));
}

// Supporting methods
private Result<User> GetUser(int userId)
{
    // Fetch user from database
    var user = _repository.FindById(userId);
    return user != null 
        ? Result<User>.Ok(user) 
        : Result<User>.Fail($"User {userId} not found");
}

private Result<User> UpdateEmail(User user, string newEmail)
{
    if (!IsValidEmail(newEmail))
        return Result<User>.Fail("Invalid email format");
    
    user.Email = newEmail;
    user.EmailVerified = false; // Require re-verification
    return Result<User>.Ok(user, "Email updated");
}

private Result<User> UpdatePhone(User user, string newPhone)
{
    if (!IsValidPhone(newPhone))
        return Result<User>.Fail("Invalid phone number");
    
    user.Phone = newPhone;
    return Result<User>.Ok(user, "Phone updated");
}
```

### Complex Pipeline Example

```csharp
public class OrderService
{
    public Result<OrderConfirmation> ProcessOrderPipeline(OrderRequest request)
    {
        var executionLog = new StringBuilder();
        
        return Result.Ok("Starting order processing")
            .Then(() =>
            {
                executionLog.AppendLine("1. Validating request");
                return ValidateRequest(request);
            })
            .Then(() =>
            {
                executionLog.AppendLine("2. Checking inventory");
                return CheckInventory(request);
            })
            .When(request.RequiresApproval, () =>
            {
                executionLog.AppendLine("3. Getting approval");
                return GetApproval(request);
            })
            .Then(() =>
            {
                executionLog.AppendLine("4. Creating order");
                return CreateOrder(request);
            })
            .Then(order =>
            {
                executionLog.AppendLine("5. Processing payment");
                return ProcessPayment(order);
            })
            .Then(order =>
            {
                executionLog.AppendLine("6. Generating confirmation");
                var confirmation = new OrderConfirmation
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    EstimatedDelivery = DateTime.Now.AddDays(3),
                    ProcessingLog = executionLog.ToString()
                };
                return Result<OrderConfirmation>.Ok(confirmation, "Order processed successfully");
            });
    }
}
```

## Async Patterns

### Basic Async Operations

```csharp
public async Task<Result<WeatherInfo>> GetWeatherAsync(string city)
{
    // Start with validation
    return await ValidateCity(city)
        .ThenAsync(async validCity =>
        {
            // Simulate API call
            await Task.Delay(100);
            var temperature = Random.Next(-10, 35);
            
            var weather = new WeatherInfo
            {
                City = validCity,
                Temperature = temperature,
                Description = GetWeatherDescription(temperature),
                UpdatedAt = DateTime.UtcNow
            };
            
            return Result<WeatherInfo>.Ok(weather, "Weather data retrieved");
        });
}

private Result<string> ValidateCity(string city)
{
    if (string.IsNullOrWhiteSpace(city))
        return Result<string>.Fail("City name is required");
    
    if (city.Length < 2)
        return Result<string>.Fail("City name too short");
    
    return Result<string>.Ok(city.Trim());
}

private string GetWeatherDescription(int temperature)
{
    return temperature switch
    {
        < 0 => "Freezing",
        < 10 => "Cold",
        < 20 => "Cool",
        < 30 => "Warm",
        _ => "Hot"
    };
}
```

### Async Pipeline with Multiple Steps

```csharp
public async Task<Result<ProcessedData>> ProcessDataAsync(string dataId)
{
    var executionOrder = new List<string>();
    
    return await Result.Ok("Starting data processing")
        .ThenAsync(async () =>
        {
            executionOrder.Add("Fetching data");
            await Task.Delay(50); // Simulate async work
            return FetchData(dataId);
        })
        .ThenAsync(async data =>
        {
            executionOrder.Add("Validating data");
            await Task.Delay(30); // Simulate async work
            return ValidateData(data);
        })
        .ThenAsync(async data =>
        {
            executionOrder.Add("Transforming data");
            await Task.Delay(40); // Simulate async work
            return TransformData(data);
        })
        .ThenAsync(async transformed =>
        {
            executionOrder.Add("Saving results");
            await Task.Delay(20); // Simulate async work
            
            var processed = new ProcessedData
            {
                Id = dataId,
                Result = transformed,
                ProcessingSteps = executionOrder,
                ProcessedAt = DateTime.UtcNow
            };
            
            return Result<ProcessedData>.Ok(processed, "Data processing completed");
        });
}
```

### Mixed Sync and Async Operations

```csharp
public async Task<Result<Report>> GenerateReportAsync(ReportRequest request)
{
    var stepResults = new StringBuilder();
    
    return await Result.Ok("Starting report generation")
        // Sync validation
        .Then(() =>
        {
            stepResults.AppendLine("1. Validated request (sync)");
            return ValidateReportRequest(request);
        })
        // Async data fetch
        .ThenAsync(async () =>
        {
            stepResults.AppendLine("2. Fetching data (async)");
            await Task.Delay(100);
            return FetchReportData(request);
        })
        // Sync transformation
        .Then(data =>
        {
            stepResults.AppendLine("3. Transforming data (sync)");
            return TransformReportData(data);
        })
        // Async generation
        .ThenAsync(async transformed =>
        {
            stepResults.AppendLine("4. Generating report (async)");
            await Task.Delay(150);
            
            var report = new Report
            {
                Title = request.Title,
                Data = transformed,
                GeneratedAt = DateTime.UtcNow,
                ProcessingLog = stepResults.ToString()
            };
            
            return Result<Report>.Ok(report, "Report generated successfully");
        });
}
```

### Working with Task<Result>

```csharp
public async Task<Result<string>> ProcessMultipleSourcesAsync()
{
    // Start multiple async operations
    Task<Result<string>> source1Task = FetchFromSource1Async();
    Task<Result<string>> source2Task = FetchFromSource2Async();
    Task<Result<string>> source3Task = FetchFromSource3Async();
    
    // Process first completed source
    var firstCompleted = await Task.WhenAny(source1Task, source2Task, source3Task);
    
    // Chain additional operations on the task result
    return await firstCompleted
        .ThenAsync(async data =>
        {
            // Process the data
            await Task.Delay(50);
            var processed = data.ToUpper();
            return Result<string>.Ok(processed, "Data processed from first available source");
        });
}

// Example of chaining on existing Task<Result>
public async Task<Result<User>> EnhanceUserDataAsync(Task<Result<int>> userIdTask)
{
    return await userIdTask
        .ThenAsync(async userId =>
        {
            // Fetch user details
            var user = await GetUserFromDatabaseAsync(userId);
            return Result<User>.Ok(user);
        })
        .ThenAsync(async user =>
        {
            // Enrich with additional data
            user.LastLoginTime = await GetLastLoginTimeAsync(user.Id);
            user.Preferences = await LoadUserPreferencesAsync(user.Id);
            return Result<User>.Ok(user, "User data enhanced");
        });
}
```

## Value Extraction

### Safe Value Extraction Patterns

```csharp
public void DemonstrateValueExtraction()
{
    // Using TryGetValue pattern
    Result<string> userNameResult = GetUserName(123);
    
    if (userNameResult.TryGetValue(out string userName))
    {
        Console.WriteLine($"Hello, {userName}!");
    }
    else
    {
        Console.WriteLine("Failed to get username");
    }
    
    // Using GetValueOrDefault
    Result<int> ageResult = GetUserAge(123);
    int displayAge = ageResult.GetValueOrDefault(-1);
    
    if (displayAge >= 0)
    {
        Console.WriteLine($"User is {displayAge} years old");
    }
    else
    {
        Console.WriteLine("Age not available");
    }
    
    // Using GetValueOrDefault with custom default
    Result<UserPreferences> prefsResult = GetUserPreferences(123);
    var preferences = prefsResult.GetValueOrDefault(UserPreferences.Default);
    
    Console.WriteLine($"Theme: {preferences.Theme}");
    Console.WriteLine($"Language: {preferences.Language}");
}

// GetValueOrThrow for critical operations
public void ProcessCriticalData()
{
    try
    {
        Result<SecurityToken> tokenResult = GenerateSecurityToken();
        
        // This will throw if the operation failed
        var token = tokenResult.GetValueOrThrow();
        
        // Use the token for critical operations
        AuthorizeRequest(token);
    }
    catch (InvalidOperationException ex)
    {
        // Handle the critical failure
        LogSecurityAlert($"Failed to generate security token: {ex.Message}");
        throw new SecurityException("Authentication failed", ex);
    }
}
```

### Null Handling

```csharp
public class NullSafeOperations
{
    // Handling nullable reference types
    public Result<string> ProcessNullableString(string? input)
    {
        if (input == null)
            return Result<string>.Fail("Input cannot be null");
        
        if (string.IsNullOrWhiteSpace(input))
            return Result<string>.Fail("Input cannot be empty");
        
        var processed = input.Trim().ToUpper();
        return Result<string>.Ok(processed, "String processed successfully");
    }
    
    // Handling nullable value types
    public Result<decimal> CalculateDiscount(decimal? originalPrice, decimal? discountPercent)
    {
        if (!originalPrice.HasValue)
            return Result<decimal>.Fail("Original price is required");
        
        if (!discountPercent.HasValue)
            return Result<decimal>.Fail("Discount percentage is required");
        
        if (originalPrice.Value <= 0)
            return Result<decimal>.Fail("Original price must be positive");
        
        if (discountPercent.Value < 0 || discountPercent.Value > 100)
            return Result<decimal>.Fail("Discount percentage must be between 0 and 100");
        
        var discount = originalPrice.Value * (discountPercent.Value / 100);
        var finalPrice = originalPrice.Value - discount;
        
        return Result<decimal>.Ok(finalPrice, $"Discount of {discountPercent}% applied");
    }
    
    // Safe extraction with null checks
    public string GetDisplayName(int userId)
    {
        var userResult = GetUser(userId);
        
        // Chain-safe null handling
        return userResult
            .Then(user => user.DisplayName != null 
                ? Result<string>.Ok(user.DisplayName) 
                : Result<string>.Fail("No display name"))
            .GetValueOrDefault($"User{userId}");
    }
}
```

## Error Aggregation

### Combining Multiple Results

```csharp
public class ValidationService
{
    public Result ValidateOrder(Order order)
    {
        var validationResults = new List<Result>
        {
            ValidateCustomerInfo(order.Customer),
            ValidateShippingAddress(order.ShippingAddress),
            ValidatePaymentMethod(order.PaymentMethod),
            ValidateOrderItems(order.Items)
        };
        
        var combined = Result.Combine(validationResults);
        
        if (!combined.Success)
        {
            // Access detailed error information
            var aggregateError = combined.Exception as AggregateError;
            
            Console.WriteLine("Validation failed with the following errors:");
            foreach (var error in aggregateError.ErrorMessages)
            {
                Console.WriteLine($"- {error}");
            }
        }
        
        return combined;
    }
    
    private Result ValidateCustomerInfo(Customer customer)
    {
        if (customer == null)
            return Result.Fail("Customer information is required");
        
        if (string.IsNullOrEmpty(customer.Email))
            return Result.Fail("Customer email is required");
        
        return Result.Ok("Customer information is valid");
    }
    
    private Result ValidateShippingAddress(Address address)
    {
        if (address == null)
            return Result.Fail("Shipping address is required");
        
        if (string.IsNullOrEmpty(address.Street))
            return Result.Fail("Street address is required");
        
        if (string.IsNullOrEmpty(address.City))
            return Result.Fail("City is required");
        
        if (string.IsNullOrEmpty(address.PostalCode))
            return Result.Fail("Postal code is required");
        
        return Result.Ok("Shipping address is valid");
    }
    
    private Result ValidatePaymentMethod(PaymentMethod payment)
    {
        if (payment == null)
            return Result.Fail("Payment method is required");
        
        return payment.Type switch
        {
            PaymentType.CreditCard => ValidateCreditCard(payment),
            PaymentType.PayPal => ValidatePayPal(payment),
            PaymentType.BankTransfer => ValidateBankTransfer(payment),
            _ => Result.Fail($"Unsupported payment type: {payment.Type}")
        };
    }
    
    private Result ValidateOrderItems(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            return Result.Fail("Order must contain at least one item");
        
        var itemResults = items.Select(ValidateOrderItem).ToList();
        return Result.Combine(itemResults);
    }
}
```

### Combining Results with Data

```csharp
public class DataAggregationService
{
    public Result<OrderSummary> AggregateOrderData(int orderId)
    {
        // Fetch data from multiple sources
        var dataResults = new List<Result<decimal>>
        {
            GetProductTotal(orderId),
            GetShippingCost(orderId),
            GetTaxAmount(orderId),
            GetDiscountAmount(orderId)
        };
        
        var combined = Result<decimal>.Combine(dataResults);
        
        if (combined.Success)
        {
            var values = combined.Data.ToList();
            var summary = new OrderSummary
            {
                OrderId = orderId,
                ProductTotal = values[0],
                ShippingCost = values[1],
                TaxAmount = values[2],
                DiscountAmount = values[3],
                GrandTotal = values[0] + values[1] + values[2] - values[3]
            };
            
            return Result<OrderSummary>.Ok(summary, "Order summary calculated");
        }
        
        return Result<OrderSummary>.Fail("Failed to aggregate order data", combined.Exception);
    }
    
    private Result<decimal> GetProductTotal(int orderId)
    {
        // Simulate calculation
        return Result<decimal>.Ok(99.99m);
    }
    
    private Result<decimal> GetShippingCost(int orderId)
    {
        // Simulate calculation
        return Result<decimal>.Ok(9.99m);
    }
    
    private Result<decimal> GetTaxAmount(int orderId)
    {
        // Simulate calculation
        return Result<decimal>.Ok(10.00m);
    }
    
    private Result<decimal> GetDiscountAmount(int orderId)
    {
        // Simulate calculation
        return Result<decimal>.Ok(5.00m);
    }
}
```

### Handling Aggregate Errors

```csharp
public class BatchProcessor
{
    public async Task<Result> ProcessBatchAsync(List<BatchItem> items)
    {
        var results = new List<Result>();
        
        foreach (var item in items)
        {
            try
            {
                var result = await ProcessItemAsync(item);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(Result.Fail($"Failed to process item {item.Id}", ex));
            }
        }
        
        var combined = Result.Combine(results);
        
        if (!combined.Success)
        {
            var aggregateError = combined.Exception as AggregateError;
            
            // Log detailed errors
            _logger.LogError("Batch processing failed with {ErrorCount} errors", 
                aggregateError.ErrorMessages.Count);
            
            foreach (var error in aggregateError.ErrorMessages)
            {
                _logger.LogError("- {Error}", error);
            }
            
            // Check if we should retry failed items
            if (ShouldRetryFailedItems(aggregateError))
            {
                return await RetryFailedItemsAsync(items, aggregateError);
            }
        }
        
        return combined;
    }
    
    private bool ShouldRetryFailedItems(AggregateError error)
    {
        // Check if any errors are retryable
        return error.InnerErrors.Any(e => 
            e is HttpRequestException || 
            e is TimeoutException ||
            e is TaskCanceledException);
    }
}
```

## Implicit Conversions

### Value to Result Conversions

```csharp
public class ImplicitConversionExamples
{
    public void DemonstrateImplicitConversions()
    {
        // Implicit conversion from value to Result<T>
        Result<string> nameResult = "John Doe";
        Result<int> ageResult = 25;
        Result<bool> activeResult = true;
        
        // All of these are successful results
        Console.WriteLine(nameResult.Success); // True
        Console.WriteLine(ageResult.Success); // True
        Console.WriteLine(activeResult.Success); // True
        
        // Using in method returns
        Result<decimal> CalculatePrice(int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                return Result<decimal>.Fail("Quantity must be positive");
            
            // Implicit conversion in return statement
            return quantity * unitPrice;
        }
        
        // Using in expressions
        Result<int> result1 = 10;
        Result<int> result2 = 20;
        
        var sum = result1.Then(v1 => 
            result2.Then(v2 => 
                v1 + v2)); // Implicit conversion of sum to Result<int>
    }
    
    public void ImplicitExtractionExamples()
    {
        // Implicit conversion from Result<T> to value (use with caution!)
        Result<int> successResult = Result<int>.Ok(42);
        int value = successResult; // Works because Success is true
        
        // This will throw InvalidOperationException
        Result<int> failedResult = Result<int>.Fail("Error");
        try
        {
            int badValue = failedResult; // Throws!
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Expected error: {ex.Message}");
        }
        
        // Safe usage pattern
        Result<string> GetConfigValue(string key)
        {
            // Fetch from configuration
            return "some-value";
        }
        
        // Only use implicit conversion when you're certain of success
        var config = GetDefaultConfig();
        string apiUrl = config.ApiUrl; // Safe if GetDefaultConfig always succeeds
    }
    
    private Result<Config> GetDefaultConfig()
    {
        // This method always returns a successful result
        return new Config { ApiUrl = "https://api.example.com" };
    }
}
```

### Practical Implicit Conversion Patterns

```csharp
public class CalculationService
{
    // Method that uses implicit conversions effectively
    public Result<Invoice> CalculateInvoice(Order order)
    {
        // Start with validations
        if (order == null)
            return Result<Invoice>.Fail("Order cannot be null");
        
        if (!order.Items.Any())
            return Result<Invoice>.Fail("Order must have items");
        
        // Use implicit conversions for calculations
        Result<decimal> subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        Result<decimal> tax = CalculateTax(subtotal);
        Result<decimal> shipping = CalculateShipping(order);
        
        // Combine results
        return subtotal.Then(sub =>
            tax.Then(t =>
                shipping.Then(ship =>
                {
                    var invoice = new Invoice
                    {
                        OrderId = order.Id,
                        Subtotal = sub,
                        Tax = t,
                        Shipping = ship,
                        Total = sub + t + ship
                    };
                    
                    // Implicit conversion in return
                    return invoice;
                })));
    }
    
    private Result<decimal> CalculateTax(Result<decimal> subtotal)
    {
        return subtotal.Then(amount =>
        {
            const decimal taxRate = 0.08m; // 8% tax
            return amount * taxRate; // Implicit conversion
        });
    }
    
    private Result<decimal> CalculateShipping(Order order)
    {
        var weight = order.Items.Sum(i => i.Weight);
        
        // Implicit conversion based on weight
        return weight switch
        {
            <= 1 => 5.99m,    // Light items
            <= 5 => 9.99m,    // Medium items
            <= 10 => 14.99m,  // Heavy items
            _ => 19.99m       // Very heavy items
        };
    }
}
```

## Real-World Scenarios

### File Processing Service

```csharp
public class FileProcessingService
{
    private readonly ILogger _logger;
    
    public async Task<Result<ProcessedFile>> ProcessFileAsync(string filePath)
    {
        return await ValidateFilePath(filePath)
            .ThenAsync(async path => await ReadFileAsync(path))
            .ThenAsync(async content => await ValidateFileContent(content))
            .ThenAsync(async content => await ProcessContentAsync(content))
            .ThenAsync(async processed => await SaveProcessedFileAsync(filePath, processed))
            .ThenAsync(async result =>
            {
                await LogSuccess(filePath);
                return Result<ProcessedFile>.Ok(result, "File processed successfully");
            });
    }
    
    private Result<string> ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Result<string>.Fail("File path cannot be empty");
        
        if (!File.Exists(filePath))
            return Result<string>.Fail($"File not found: {filePath}");
        
        var extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".txt" && extension != ".csv")
            return Result<string>.Fail($"Unsupported file type: {extension}");
        
        return Result<string>.Ok(filePath);
    }
    
    private async Task<Result<string>> ReadFileAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrWhiteSpace(content))
                return Result<string>.Fail("File is empty");
            
            return Result<string>.Ok(content, "File read successfully");
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Failed to read file: {ex.Message}", ex);
        }
    }
    
    private async Task<Result<string>> ValidateFileContent(string content)
    {
        await Task.Delay(10); // Simulate async validation
        
        var lines = content.Split('\n');
        if (lines.Length < 2)
            return Result<string>.Fail("File must contain at least 2 lines");
        
        // Additional validation logic here
        
        return Result<string>.Ok(content, "Content validation passed");
    }
    
    private async Task<Result<ProcessedContent>> ProcessContentAsync(string content)
    {
        await Task.Delay(50); // Simulate processing
        
        var processed = new ProcessedContent
        {
            OriginalSize = content.Length,
            ProcessedLines = content.Split('\n').Length,
            ProcessedAt = DateTime.UtcNow,
            // Additional processing...
        };
        
        return Result<ProcessedContent>.Ok(processed);
    }
    
    private async Task<Result<ProcessedFile>> SaveProcessedFileAsync(
        string originalPath, 
        ProcessedContent processed)
    {
        try
        {
            var outputPath = Path.ChangeExtension(originalPath, ".processed");
            await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(processed));
            
            var result = new ProcessedFile
            {
                OriginalPath = originalPath,
                ProcessedPath = outputPath,
                ProcessedContent = processed
            };
            
            return Result<ProcessedFile>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<ProcessedFile>.Fail("Failed to save processed file", ex);
        }
    }
}
```

### API Client with Retry Logic

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly int _maxRetries = 3;
    
    public async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Result<T>.Fail($"API error: {response.StatusCode} - {content}");
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<T>(json);
            
            if (data == null)
                return Result<T>.Fail("Failed to deserialize response");
            
            return Result<T>.Ok(data, "API call successful");
        });
    }
    
    private async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> operation)
    {
        Result<T> lastResult = Result<T>.Fail("Operation not executed");
        
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                lastResult = await operation();
                
                if (lastResult.Success)
                    return lastResult;
                
                // Check if error is retryable
                if (!IsRetryableError(lastResult))
                    return lastResult;
                
                // Wait before retry with exponential backoff
                if (attempt < _maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
            catch (HttpRequestException ex)
            {
                lastResult = Result<T>.Fail($"Network error on attempt {attempt}", ex);
                
                if (attempt < _maxRetries)
                    await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
            catch (TaskCanceledException ex)
            {
                return Result<T>.Fail("Request timed out", ex);
            }
        }
        
        return Result<T>.Fail(
            $"Operation failed after {_maxRetries} attempts", 
            lastResult.Exception);
    }
    
    private bool IsRetryableError<T>(Result<T> result)
    {
        // Don't retry client errors (4xx)
        if (result.Message.Contains("400") || 
            result.Message.Contains("401") ||
            result.Message.Contains("403") ||
            result.Message.Contains("404"))
        {
            return false;
        }
        
        // Retry server errors (5xx) and network errors
        return true;
    }
}
```

### Database Transaction Pattern

```csharp
public class OrderRepository
{
    private readonly IDbConnection _connection;
    
    public async Task<Result<Order>> CreateOrderWithTransactionAsync(
        CreateOrderCommand command)
    {
        using var transaction = _connection.BeginTransaction();
        
        try
        {
            var result = await ValidateCommand(command)
                .ThenAsync(async cmd => await CreateOrderRecord(cmd, transaction))
                .ThenAsync(async order => await CreateOrderItems(order, command.Items, transaction))
                .ThenAsync(async order => await UpdateInventory(command.Items, transaction))
                .ThenAsync(async _ => await CreateAuditLog(command, transaction))
                .ThenAsync(async _ =>
                {
                    await transaction.CommitAsync();
                    return await GetOrderById(command.OrderId);
                });
            
            if (!result.Success)
            {
                await transaction.RollbackAsync();
            }
            
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<Order>.Fail("Transaction failed", ex);
        }
    }
    
    private Result<CreateOrderCommand> ValidateCommand(CreateOrderCommand command)
    {
        if (command == null)
            return Result<CreateOrderCommand>.Fail("Command cannot be null");
        
        if (string.IsNullOrEmpty(command.CustomerId))
            return Result<CreateOrderCommand>.Fail("Customer ID is required");
        
        if (!command.Items.Any())
            return Result<CreateOrderCommand>.Fail("Order must have items");
        
        return Result<CreateOrderCommand>.Ok(command);
    }
    
    private async Task<Result<Order>> CreateOrderRecord(
        CreateOrderCommand command, 
        IDbTransaction transaction)
    {
        try
        {
            var order = new Order
            {
                Id = command.OrderId,
                CustomerId = command.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };
            
            await _connection.ExecuteAsync(
                @"INSERT INTO Orders (Id, CustomerId, OrderDate, Status) 
                  VALUES (@Id, @CustomerId, @OrderDate, @Status)",
                order,
                transaction);
            
            return Result<Order>.Ok(order);
        }
        catch (Exception ex)
        {
            return Result<Order>.Fail("Failed to create order record", ex);
        }
    }
}
```

## Integration Patterns

### ASP.NET Core Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        
        return result.Success
            ? Ok(result.Data)
            : NotFound(new { error = result.Message });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await _userService.CreateUserAsync(dto);
        
        if (result.Success)
        {
            return CreatedAtAction(
                nameof(GetUser), 
                new { id = result.Data.Id }, 
                result.Data);
        }
        
        // Handle different error types
        if (result.Message.Contains("already exists"))
            return Conflict(new { error = result.Message });
        
        if (result.Message.Contains("validation"))
            return BadRequest(new { error = result.Message });
        
        return StatusCode(500, new { error = "An error occurred" });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);
        
        return result.Success
            ? NoContent()
            : result.Message.Contains("not found")
                ? NotFound(new { error = result.Message })
                : BadRequest(new { error = result.Message });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        
        return result.Success
            ? NoContent()
            : NotFound(new { error = result.Message });
    }
}

// Global exception handler that works with Results
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            var result = Result.Fail("An unexpected error occurred", ex);
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = result.Message,
                details = context.RequestServices
                    .GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                    ? ex.ToString()
                    : null
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
```

### Minimal API Integration

```csharp
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();
        
        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .Produces<List<UserDto>>();
        
        group.MapGet("/{id:int}", GetUserById)
            .WithName("GetUserById")
            .Produces<UserDto>()
            .ProducesProblem(404);
        
        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .Produces<UserDto>(201)
            .ProducesValidationProblem();
        
        group.MapPut("/{id:int}", UpdateUser)
            .WithName("UpdateUser")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesValidationProblem();
        
        group.MapDelete("/{id:int}", DeleteUser)
            .WithName("DeleteUser")
            .Produces(204)
            .ProducesProblem(404);
    }
    
    private static async Task<IResult> GetAllUsers(IUserService service)
    {
        var result = await service.GetAllUsersAsync();
        
        return result.Success
            ? Results.Ok(result.Data)
            : Results.Problem(result.Message);
    }
    
    private static async Task<IResult> GetUserById(
        int id, 
        IUserService service)
    {
        var result = await service.GetUserByIdAsync(id);
        
        return result.Success
            ? Results.Ok(result.Data)
            : Results.NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = result.Message,
                Status = 404
            });
    }
    
    private static async Task<IResult> CreateUser(
        CreateUserDto dto, 
        IUserService service,
        IValidator<CreateUserDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        var result = await service.CreateUserAsync(dto);
        
        if (result.Success)
        {
            return Results.CreatedAtRoute(
                "GetUserById",
                new { id = result.Data.Id },
                result.Data);
        }
        
        return result.Message.Contains("already exists")
            ? Results.Conflict(new ProblemDetails
            {
                Title = "User already exists",
                Detail = result.Message,
                Status = 409
            })
            : Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = result.Message,
                Status = 400
            });
    }
}
```

### MediatR Integration

```csharp
// Command definition
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity) : IRequest<Result<ProductDto>>;

// Command handler
public class CreateProductCommandHandler 
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductCommand> _validator;
    
    public CreateProductCommandHandler(
        IProductRepository repository,
        IValidator<CreateProductCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }
    
    public async Task<Result<ProductDto>> Handle(
        CreateProductCommand request, 
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<ProductDto>.Fail($"Validation failed: {errors}");
        }
        
        // Check for duplicates
        var existing = await _repository.GetByNameAsync(request.Name, cancellationToken);
        if (existing != null)
        {
            return Result<ProductDto>.Fail($"Product '{request.Name}' already exists");
        }
        
        // Create product
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };
        
        // Save to database
        var saved = await _repository.AddAsync(product, cancellationToken);
        if (saved == null)
        {
            return Result<ProductDto>.Fail("Failed to save product to database");
        }
        
        // Map to DTO
        var dto = new ProductDto
        {
            Id = saved.Id,
            Name = saved.Name,
            Description = saved.Description,
            Price = saved.Price,
            StockQuantity = saved.StockQuantity
        };
        
        return Result<ProductDto>.Ok(dto, "Product created successfully");
    }
}

// Query definition
public record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;

// Query handler
public class GetProductByIdQueryHandler 
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    
    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
        {
            return Result<ProductDto>.Fail($"Product with ID {request.Id} not found");
        }
        
        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };
        
        return Result<ProductDto>.Ok(dto, "Product retrieved successfully");
    }
}

// Controller using MediatR
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        
        return result.Success
            ? Ok(result.Data)
            : NotFound(new { error = result.Message });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = result.Data.Id },
                result.Data);
        }
        
        return result.Message.Contains("already exists")
            ? Conflict(new { error = result.Message })
            : BadRequest(new { error = result.Message });
    }
}
```

## Best Practices

### 1. Use Results for Expected Failures

```csharp
// Good: Using Result for expected business logic failures
public Result<decimal> CalculateDiscount(decimal price, string couponCode)
{
    if (price <= 0)
        return Result<decimal>.Fail("Price must be positive");
    
    var coupon = _couponRepository.FindByCode(couponCode);
    if (coupon == null)
        return Result<decimal>.Fail("Invalid coupon code");
    
    if (coupon.IsExpired)
        return Result<decimal>.Fail("Coupon has expired");
    
    if (price < coupon.MinimumPurchase)
        return Result<decimal>.Fail($"Minimum purchase of {coupon.MinimumPurchase:C} required");
    
    var discount = price * coupon.DiscountPercentage;
    return Result<decimal>.Ok(discount, $"Discount of {discount:C} applied");
}

// Bad: Using exceptions for expected failures
public decimal CalculateDiscountBad(decimal price, string couponCode)
{
    if (price <= 0)
        throw new ArgumentException("Price must be positive");
    
    var coupon = _couponRepository.FindByCode(couponCode);
    if (coupon == null)
        throw new InvalidOperationException("Invalid coupon code");
    
    // This makes the calling code complex with try-catch blocks
}
```

### 2. Chain Operations Instead of Nested Ifs

```csharp
// Good: Using operation chaining
public async Task<Result<PaymentReceipt>> ProcessPaymentAsync(PaymentRequest request)
{
    return await ValidatePaymentRequest(request)
        .ThenAsync(async req => await AuthorizePayment(req))
        .ThenAsync(async auth => await CapturePayment(auth))
        .ThenAsync(async payment => await GenerateReceipt(payment))
        .ThenAsync(async receipt => await SendReceiptEmail(receipt));
}

// Bad: Nested if statements
public async Task<PaymentReceipt> ProcessPaymentBad(PaymentRequest request)
{
    var validationResult = ValidatePaymentRequest(request);
    if (validationResult.Success)
    {
        var authResult = await AuthorizePayment(request);
        if (authResult.Success)
        {
            var captureResult = await CapturePayment(authResult.Data);
            if (captureResult.Success)
            {
                // Gets deeply nested quickly
            }
        }
    }
    // Error handling becomes complex
}
```

### 3. Provide Clear, Actionable Error Messages

```csharp
// Good: Clear, specific error messages
public Result<User> CreateUser(CreateUserRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
        return Result<User>.Fail("Email address is required");
    
    if (!IsValidEmail(request.Email))
        return Result<User>.Fail($"'{request.Email}' is not a valid email address format");
    
    if (request.Password.Length < 8)
        return Result<User>.Fail("Password must be at least 8 characters long");
    
    if (!request.Password.Any(char.IsDigit))
        return Result<User>.Fail("Password must contain at least one number");
    
    // Clear success message too
    return Result<User>.Ok(user, $"User account created for {request.Email}");
}

// Bad: Vague error messages
public Result<User> CreateUserBad(CreateUserRequest request)
{
    if (!IsValid(request))
        return Result<User>.Fail("Invalid request"); // What's invalid?
    
    return Result<User>.Fail("Error"); // Completely unhelpful
}
```

### 4. Use Type-Specific Results

```csharp
// Good: Specific result types for different operations
public interface IEmailService
{
    Task<Result<EmailSentConfirmation>> SendEmailAsync(EmailMessage message);
    Task<Result<EmailTemplate>> GetTemplateAsync(string templateId);
    Task<Result<List<EmailLog>>> GetEmailHistoryAsync(string userId);
}

public class EmailSentConfirmation
{
    public string MessageId { get; set; }
    public DateTime SentAt { get; set; }
    public string Recipient { get; set; }
}

// This makes the API self-documenting and type-safe
```

### 5. Handle Aggregate Errors Appropriately

```csharp
// Good: Detailed aggregate error handling
public async Task<Result> ProcessBulkOrdersAsync(List<Order> orders)
{
    var results = new List<Result>();
    
    foreach (var order in orders)
    {
        results.Add(await ProcessOrderAsync(order));
    }
    
    var combined = Result.Combine(results);
    
    if (!combined.Success)
    {
        var aggregateError = combined.Exception as AggregateError;
        
        // Log summary
        _logger.LogWarning(
            "Bulk processing completed with {SuccessCount}/{TotalCount} successful orders",
            results.Count(r => r.Success),
            results.Count);
        
        // Log individual errors
        foreach (var error in aggregateError.ErrorMessages)
        {
            _logger.LogError("Order processing failed: {Error}", error);
        }
        
        // Decide on overall result
        var successRate = results.Count(r => r.Success) / (double)results.Count;
        if (successRate >= 0.8) // 80% success threshold
        {
            return Result.Ok($"Bulk processing completed with {results.Count(r => !r.Success)} errors");
        }
    }
    
    return combined;
}
```

### 6. Create Domain-Specific Extensions

```csharp
// Create extensions for common patterns in your domain
public static class ResultExtensions
{
    // Validation extension
    public static Result<T> Validate<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
    {
        if (!result.Success)
            return result;
        
        return predicate(result.Data) 
            ? result 
            : Result<T>.Fail(errorMessage);
    }
    
    // Logging extension
    public static Result<T> LogOnError<T>(this Result<T> result, ILogger logger)
    {
        if (!result.Success)
        {
            logger.LogError("Operation failed: {Message}", result.Message);
            if (result.Exception != null)
            {
                logger.LogError(result.Exception, "Exception details");
            }
        }
        
        return result;
    }
    
    // Retry extension
    public static async Task<Result<T>> RetryOnFailureAsync<T>(
        this Task<Result<T>> operation, 
        int maxRetries = 3,
        TimeSpan? delay = null)
    {
        var retryDelay = delay ?? TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            var result = await operation;
            if (result.Success)
                return result;
            
            if (i < maxRetries - 1)
                await Task.Delay(retryDelay);
        }
        
        return await operation; // Return last failure
    }
}

// Usage
var result = await GetUserAsync(userId)
    .Validate(user => user.IsActive, "User account is not active")
    .LogOnError(_logger);
```

### 7. Test Result-Based Code Effectively

```csharp
[TestClass]
public class UserServiceTests
{
    private readonly IUserService _userService;
    
    [TestMethod]
    public async Task CreateUser_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            Name = "Test User"
        };
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(request.Email, result.Data.Email);
        Assert.AreEqual("User created successfully", result.Message);
    }
    
    [TestMethod]
    public async Task CreateUser_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existingEmail = "existing@example.com";
        await CreateTestUser(existingEmail);
        
        var request = new CreateUserRequest
        {
            Email = existingEmail,
            Password = "SecurePass123!",
            Name = "Test User"
        };
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Message.Contains("already exists"));
    }
    
    [TestMethod]
    public async Task CreateUser_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "notanemail",
            Password = "SecurePass123!",
            Name = "Test User"
        };
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("valid email"));
    }
}
```

### 8. Document Result-Based APIs

```csharp
/// <summary>
/// Processes a payment for the given order.
/// </summary>
/// <param name="orderId">The ID of the order to process payment for.</param>
/// <param name="paymentMethod">The payment method to use.</param>
/// <returns>
/// A Result containing PaymentConfirmation on success, or an error message on failure.
/// 
/// Possible failure reasons:
/// - Order not found
/// - Order already paid
/// - Invalid payment method
/// - Insufficient funds
/// - Payment gateway error
/// </returns>
public async Task<Result<PaymentConfirmation>> ProcessPaymentAsync(
    int orderId, 
    PaymentMethod paymentMethod)
{
    // Implementation
}
```

Remember: The Result pattern is about making error handling explicit, composable, and maintainable. Use it to create more robust and predictable applications!