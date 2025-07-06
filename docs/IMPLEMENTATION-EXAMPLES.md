# Implementation Examples for Tethys.Results Enhancements

## Example: Implementing the Match Method (Phase 2)

Here's an example implementation for the `Match` method that would be added to both `Result` and `Result<T>`:

### For Result Class

```csharp
public TResult Match<TResult>(
    Func<TResult> onSuccess,
    Func<Exception, TResult> onFailure)
{
    if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
    if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
    
    return Success ? onSuccess() : onFailure(Exception);
}

// Async version
public async Task<TResult> MatchAsync<TResult>(
    Func<Task<TResult>> onSuccess,
    Func<Exception, Task<TResult>> onFailure)
{
    if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
    if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
    
    return Success ? await onSuccess() : await onFailure(Exception);
}
```

### For Result<T> Class

```csharp
public TResult Match<TResult>(
    Func<T, TResult> onSuccess,
    Func<Exception, TResult> onFailure)
{
    if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
    if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
    
    return Success ? onSuccess(Value) : onFailure(Exception);
}

// Async version
public async Task<TResult> MatchAsync<TResult>(
    Func<T, Task<TResult>> onSuccess,
    Func<Exception, Task<TResult>> onFailure)
{
    if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
    if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
    
    return Success ? await onSuccess(Value) : await onFailure(Exception);
}
```

### Usage Examples

```csharp
// Example 1: Converting Result to HTTP response
public IActionResult HandleUserResult(Result<User> userResult)
{
    return userResult.Match(
        onSuccess: user => Ok(user),
        onFailure: error => NotFound(error.Message)
    );
}

// Example 2: Logging different outcomes
public void ProcessOrderWithLogging(Result<Order> orderResult)
{
    var logMessage = orderResult.Match(
        onSuccess: order => $"Order {order.Id} processed successfully",
        onFailure: error => $"Order processing failed: {error.Message}"
    );
    
    logger.LogInformation(logMessage);
}

// Example 3: Async match with different return paths
public async Task<string> GetUserDisplayName(int userId)
{
    var userResult = await GetUserAsync(userId);
    
    return await userResult.MatchAsync(
        onSuccess: async user => await FormatUserNameAsync(user),
        onFailure: async error => await GetDefaultDisplayNameAsync()
    );
}
```

### Test Examples

```csharp
[Test]
public void Match_WithSuccessResult_CallsOnSuccessFunction()
{
    // Arrange
    var result = Result<int>.Ok(42);
    var successCalled = false;
    var failureCalled = false;
    
    // Act
    var matchResult = result.Match(
        onSuccess: value => { successCalled = true; return value.ToString(); },
        onFailure: error => { failureCalled = true; return "error"; }
    );
    
    // Assert
    Assert.That(successCalled, Is.True);
    Assert.That(failureCalled, Is.False);
    Assert.That(matchResult, Is.EqualTo("42"));
}

[Test]
public void Match_WithFailureResult_CallsOnFailureFunction()
{
    // Arrange
    var exception = new InvalidOperationException("Test error");
    var result = Result<int>.Fail(exception);
    var successCalled = false;
    var failureCalled = false;
    
    // Act
    var matchResult = result.Match(
        onSuccess: value => { successCalled = true; return value.ToString(); },
        onFailure: error => { failureCalled = true; return error.Message; }
    );
    
    // Assert
    Assert.That(successCalled, Is.False);
    Assert.That(failureCalled, Is.True);
    Assert.That(matchResult, Is.EqualTo("Test error"));
}
```

## Example: Implementing the Map Method

### For Result<T> Class

```csharp
public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
{
    if (mapper == null) throw new ArgumentNullException(nameof(mapper));
    
    return Success 
        ? Result<TNew>.Ok(mapper(Value), Message) 
        : Result<TNew>.Fail(Exception);
}

// Async version
public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
{
    if (mapper == null) throw new ArgumentNullException(nameof(mapper));
    
    return Success 
        ? Result<TNew>.Ok(await mapper(Value), Message) 
        : Result<TNew>.Fail(Exception);
}
```

### Usage Example

```csharp
// Transform a User to UserDto
Result<User> userResult = GetUser(id);
Result<UserDto> dtoResult = userResult.Map(user => new UserDto 
{
    Id = user.Id,
    Name = user.Name,
    Email = user.Email
});

// Chain multiple transformations
var result = GetProduct(productId)
    .Map(product => product.Price)
    .Map(price => price * 1.2m) // Add 20% tax
    .Map(totalPrice => $"${totalPrice:F2}");
```

## Example: Implementing IEquatable

### For Result Class

```csharp
public sealed class Result : IResult, IEquatable<Result>
{
    // ... existing code ...
    
    public bool Equals(Result other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Success == other.Success &&
               Message == other.Message &&
               ExceptionsEqual(Exception, other.Exception);
    }
    
    public override bool Equals(object obj) => Equals(obj as Result);
    
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Success.GetHashCode();
            hash = hash * 23 + (Message?.GetHashCode() ?? 0);
            hash = hash * 23 + (Exception?.GetType().GetHashCode() ?? 0);
            return hash;
        }
    }
    
    public static bool operator ==(Result left, Result right) => 
        Equals(left, right);
    
    public static bool operator !=(Result left, Result right) => 
        !Equals(left, right);
    
    private static bool ExceptionsEqual(Exception ex1, Exception ex2)
    {
        if (ex1 is null && ex2 is null) return true;
        if (ex1 is null || ex2 is null) return false;
        
        return ex1.GetType() == ex2.GetType() &&
               ex1.Message == ex2.Message;
    }
}
```

## Performance Optimization Example

### Optimizing Combine Method

```csharp
// Current implementation (multiple enumerations)
public static Result Combine(IEnumerable<Result> results)
{
    var resultsList = results.ToList(); // First enumeration
    if (resultsList.Any(r => !r.Success)) // Second enumeration
    {
        var errors = resultsList
            .Where(r => !r.Success) // Third enumeration
            .Select(r => r.Exception)
            .ToList();
        // ... rest of implementation
    }
}

// Optimized implementation (single enumeration)
public static Result Combine(IEnumerable<Result> results)
{
    var errors = new List<Exception>();
    var hasSuccess = false;
    
    foreach (var result in results)
    {
        if (result.Success)
        {
            hasSuccess = true;
        }
        else if (result.Exception != null)
        {
            errors.Add(result.Exception);
        }
    }
    
    if (errors.Count == 0)
    {
        return Ok();
    }
    
    return errors.Count == 1 
        ? Fail(errors[0]) 
        : Fail(new AggregateError(errors));
}
```

## Benchmark Example Setup

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class ResultBenchmarks
{
    [Benchmark]
    public Result CreateSuccess() => Result.Ok();
    
    [Benchmark]
    public Result CreateFailure() => Result.Fail("Error message");
    
    [Benchmark]
    public Result ChainOperations()
    {
        return Result.Ok()
            .Then(() => Result.Ok())
            .Then(() => Result.Ok())
            .Then(() => Result.Ok());
    }
    
    [Benchmark]
    public async Task<Result> AsyncChainOperations()
    {
        return await Result.Ok()
            .ThenAsync(() => Task.FromResult(Result.Ok()))
            .ThenAsync(() => Task.FromResult(Result.Ok()))
            .ThenAsync(() => Task.FromResult(Result.Ok()));
    }
    
    [Benchmark]
    [Arguments(10)]
    [Arguments(100)]
    [Arguments(1000)]
    public Result CombineResults(int count)
    {
        var results = Enumerable.Range(0, count)
            .Select(i => i % 10 == 0 ? Result.Fail($"Error {i}") : Result.Ok())
            .ToList();
            
        return Result.Combine(results);
    }
}
```