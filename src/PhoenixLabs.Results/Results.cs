using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhoenixLabs.Results;

/// <summary>
/// Represents an exception that aggregates multiple errors.
/// </summary>
public class AggregateError : Exception
{
    /// <summary>
    /// Gets the collection of inner errors.
    /// </summary>
    public IReadOnlyList<Exception> InnerErrors { get; }
    
    /// <summary>
    /// Gets the collection of error messages.
    /// </summary>
    public IReadOnlyList<string> ErrorMessages { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError"/> class with the specified error messages.
    /// </summary>
    /// <param name="errorMessages">The collection of error messages.</param>
    public AggregateError(IEnumerable<string> errorMessages)
        : base(string.Join(Environment.NewLine, errorMessages))
    {
        ErrorMessages = errorMessages.ToList().AsReadOnly();
        InnerErrors = new List<Exception>().AsReadOnly();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError"/> class with the specified inner exceptions.
    /// </summary>
    /// <param name="innerExceptions">The collection of inner exceptions.</param>
    public AggregateError(IEnumerable<Exception> innerExceptions)
        : base(string.Join(Environment.NewLine, innerExceptions.Select(e => e.Message)))
    {
        InnerErrors = innerExceptions.ToList().AsReadOnly();
        ErrorMessages = InnerErrors.Select(e => e.Message).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError"/> class with the specified message and inner exceptions.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerExceptions">The collection of inner exceptions.</param>
    public AggregateError(string message, IEnumerable<Exception> innerExceptions)
        : base(message)
    {
        InnerErrors = innerExceptions.ToList().AsReadOnly();
        ErrorMessages = new List<string> { message }.Concat(InnerErrors.Select(e => e.Message)).ToList().AsReadOnly();
    }
}

/// <summary>
/// Base interface for all result types, providing a common abstraction for operation outcomes.
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    bool Success { get; }

    /// <summary>
    /// Gets a descriptive message about the operation result.
    /// </summary>
    /// <value>A success message when <see cref="Success"/> is <c>true</c>, or an error message when <see cref="Success"/> is <c>false</c>.</value>
    string Message { get; }

    /// <summary>
    /// Gets the exception that caused the operation to fail, if any.
    /// </summary>
    /// <value>The exception that caused the failure, or <c>null</c> if the operation succeeded or failed without an exception.</value>
    Exception Exception { get; }
}

/// <summary>
/// Represents the result of an operation that returns data of type <typeparamref name="T"/>.
/// This class is immutable and thread-safe.
/// </summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
public sealed class Result<T> : IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    public bool Success { get; }

    /// <summary>
    /// Gets a descriptive message about the operation result.
    /// </summary>
    /// <value>A success message when <see cref="Success"/> is <c>true</c>, or an error message when <see cref="Success"/> is <c>false</c>.</value>
    public string Message { get; }

    /// <summary>
    /// Gets the data returned by the operation.
    /// </summary>
    /// <value>The data when <see cref="Success"/> is <c>true</c>, or the default value of <typeparamref name="T"/> when <see cref="Success"/> is <c>false</c>.</value>
    public T Data { get; }

    /// <summary>
    /// Gets the exception that caused the operation to fail, if any.
    /// </summary>
    /// <value>The exception that caused the failure, or <c>null</c> if the operation succeeded or failed without an exception.</value>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="success">A value indicating whether the operation was successful.</param>
    /// <param name="message">A descriptive message about the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that caused the operation to fail, if any.</param>
    private Result(bool success, string message, T data, Exception exception = null)
    {
        Success = success;
        Message = message ?? (success ? "Operation completed successfully" : "Operation failed");
        Data = data;
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful result with the specified data and default success message.
    /// </summary>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new <see cref="Result{T}"/> instance representing a successful operation.</returns>
    public static Result<T> Ok(T data)
    {
        return new Result<T>(true, "Operation completed successfully", data);
    }
    
    /// <summary>
    /// Creates a successful result with the specified data and message.
    /// </summary>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="message">A descriptive message about the operation result.</param>
    /// <returns>A new <see cref="Result{T}"/> instance representing a successful operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result<T> Ok(T data, string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Success message cannot be null");
        }
        
        return new Result<T>(true, message, data);
    }

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="message">A descriptive error message about why the operation failed.</param>
    /// <returns>A new <see cref="Result{T}"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result<T> Fail(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Error message cannot be null");
        }
        
        return new Result<T>(false, message, default, null);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and exception.
    /// </summary>
    /// <param name="message">A descriptive error message about why the operation failed.</param>
    /// <param name="exception">The exception that caused the operation to fail.</param>
    /// <returns>A new <see cref="Result{T}"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result<T> Fail(string message, Exception exception)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Error message cannot be null");
        }
        
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception cannot be null");
        }
        
        return new Result<T>(false, message, default, exception);
    }
    
    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the operation to fail.</param>
    /// <returns>A new <see cref="Result{T}"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static Result<T> FromException(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception cannot be null");
        }
        
        return Fail(exception.Message, exception);
    }
    
    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results">The collection of results to combine.</param>
    /// <returns>
    /// A successful result if all results are successful;
    /// otherwise, a failed result with an <see cref="AggregateError"/> containing all error messages and exceptions.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="results"/> is empty.</exception>
    public static Result<IEnumerable<T>> Combine(IEnumerable<Result<T>> results)
    {
        if (results == null)
        {
            throw new ArgumentNullException(nameof(results), "Results collection cannot be null");
        }
        
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            throw new ArgumentException("Results collection cannot be empty", nameof(results));
        }
        
        var failedResults = resultsList.Where(r => !r.Success).ToList();
        if (failedResults.Count == 0)
        {
            // Create a successful result with the collection of data values
            return Result<IEnumerable<T>>.Ok(resultsList.Select(r => r.Data), "All operations completed successfully");
        }
        
        var errorMessages = failedResults.Select(r => r.Message).ToList();
        var exceptions = failedResults.Where(r => r.Exception != null).Select(r => r.Exception).ToList();
        
        var aggregateError = exceptions.Count > 0
            ? new AggregateError(errorMessages.First(), exceptions)
            : new AggregateError(errorMessages);
        
        return Result<IEnumerable<T>>.Fail("One or more operations failed", aggregateError);
    }
    
    /// <summary>
    /// Converts this generic result to a non-generic result, discarding the data.
    /// </summary>
    /// <returns>A new <see cref="Result"/> instance with the same success state, message, and exception as this result.</returns>
    public Result AsResult()
    {
        return Success 
            ? Result.Ok(Message) 
            : Result.Fail(Message, Exception);
    }
    
    /// <summary>
    /// Gets the value from the result if successful, or returns the specified default value if not.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is not successful. Defaults to the default value of <typeparamref name="T"/>.</param>
    /// <returns>The data value if the result is successful; otherwise, the specified default value.</returns>
    public T GetValueOrDefault(T defaultValue = default)
    {
        return Success ? Data : defaultValue;
    }
    
    /// <summary>
    /// Tries to get the value from the result.
    /// </summary>
    /// <param name="value">When this method returns, contains the value from the result if the result is successful, or the default value of <typeparamref name="T"/> if the result is not successful.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(out T value)
    {
        value = Success ? Data : default;
        return Success;
    }
    
    /// <summary>
    /// Gets the value from the result if successful, or throws an exception if not.
    /// </summary>
    /// <returns>The data value if the result is successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is not successful.</exception>
    public T GetValueOrThrow()
    {
        if (!Success)
        {
            throw Exception ?? new InvalidOperationException(Message);
        }
        
        return Data;
    }
    
    /// <summary>
    /// Implicitly converts a <see cref="Result{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>The data value if the result is successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is not successful.</exception>
    public static implicit operator T(Result<T> result)
    {
        return result.GetValueOrThrow();
    }
    
    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the value.</returns>
    public static implicit operator Result<T>(T value)
    {
        return Ok(value);
    }
}

/// <summary>
/// Represents the result of an operation that does not return any data.
/// This class is immutable and thread-safe.
/// </summary>
public sealed class Result : IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    public bool Success { get; }

    /// <summary>
    /// Gets a descriptive message about the operation result.
    /// </summary>
    /// <value>A success message when <see cref="Success"/> is <c>true</c>, or an error message when <see cref="Success"/> is <c>false</c>.</value>
    public string Message { get; }

    /// <summary>
    /// Gets the exception that caused the operation to fail, if any.
    /// </summary>
    /// <value>The exception that caused the failure, or <c>null</c> if the operation succeeded or failed without an exception.</value>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">A value indicating whether the operation was successful.</param>
    /// <param name="message">A descriptive message about the operation result.</param>
    /// <param name="exception">The exception that caused the operation to fail, if any.</param>
    private Result(bool success, string message, Exception exception = null)
    {
        Success = success;
        Message = message ?? (success ? "Operation completed successfully" : "Operation failed");
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful result with the default success message.
    /// </summary>
    /// <returns>A new <see cref="Result"/> instance representing a successful operation.</returns>
    public static Result Ok()
    {
        return new Result(true, "Operation completed successfully");
    }
    
    /// <summary>
    /// Creates a successful result with the specified message.
    /// </summary>
    /// <param name="message">A descriptive message about the operation result.</param>
    /// <returns>A new <see cref="Result"/> instance representing a successful operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result Ok(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Success message cannot be null");
        }
        
        return new Result(true, message);
    }

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="message">A descriptive error message about why the operation failed.</param>
    /// <returns>A new <see cref="Result"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result Fail(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Error message cannot be null");
        }
        
        return new Result(false, message, null);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and exception.
    /// </summary>
    /// <param name="message">A descriptive error message about why the operation failed.</param>
    /// <param name="exception">The exception that caused the operation to fail.</param>
    /// <returns>A new <see cref="Result"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static Result Fail(string message, Exception exception)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Error message cannot be null");
        }
        
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception cannot be null");
        }
        
        return new Result(false, message, exception);
    }
    
    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the operation to fail.</param>
    /// <returns>A new <see cref="Result"/> instance representing a failed operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static Result FromException(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception), "Exception cannot be null");
        }
        
        return Fail(exception.Message, exception);
    }
    
    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results">The collection of results to combine.</param>
    /// <returns>
    /// A successful result if all results are successful;
    /// otherwise, a failed result with an <see cref="AggregateError"/> containing all error messages and exceptions.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="results"/> is empty.</exception>
    public static Result Combine(IEnumerable<Result> results)
    {
        if (results == null)
        {
            throw new ArgumentNullException(nameof(results), "Results collection cannot be null");
        }
        
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            throw new ArgumentException("Results collection cannot be empty", nameof(results));
        }
        
        var failedResults = resultsList.Where(r => !r.Success).ToList();
        if (failedResults.Count == 0)
        {
            return Ok("All operations completed successfully");
        }
        
        var errorMessages = failedResults.Select(r => r.Message).ToList();
        var exceptions = failedResults.Where(r => r.Exception != null).Select(r => r.Exception).ToList();
        
        var aggregateError = exceptions.Count > 0
            ? new AggregateError(errorMessages.First(), exceptions)
            : new AggregateError(errorMessages);
        
        return Fail("One or more operations failed", aggregateError);
    }
}

/// <summary>
/// Provides extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> instances.
/// </summary>
public static class ResultExtensions
{
    #region Synchronous Methods
    
    /// <summary>
    /// Executes the next operation only if the current result is successful.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="next">A function that returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result.
    /// </returns>
    public static Result Then(this Result result, Func<Result> next)
    {
        return result.Success ? next() : result;
    }
    
    /// <summary>
    /// Executes the next operation with data from the current result, only if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the current result.</typeparam>
    /// <param name="result">The current result containing data.</param>
    /// <param name="next">A function that takes the data from the current result and returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result converted to a non-generic result.
    /// </returns>
    public static Result Then<T>(this Result<T> result, Func<T, Result> next)
    {
        return result.Success ? next(result.Data) : result.AsResult();
    }
    
    /// <summary>
    /// Executes the next operation with data from the current result and produces new data, only if the current result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of data in the current result.</typeparam>
    /// <typeparam name="TOut">The type of data in the next result.</typeparam>
    /// <param name="result">The current result containing data.</param>
    /// <param name="next">A function that takes the data from the current result and returns a new result with different data.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, a failed result with the same error details as the current result.
    /// </returns>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next)
    {
        return result.Success ? next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
    }
    
    /// <summary>
    /// Executes an operation if a condition is true and the current result is successful.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="operation">A function that returns the next result.</param>
    /// <returns>
    /// The result of the operation if the current result is successful and the condition is true;
    /// otherwise, the current result.
    /// </returns>
    public static Result When(this Result result, bool condition, Func<Result> operation)
    {
        return !result.Success ? result : 
                condition ? operation() : result;
    }
    
    /// <summary>
    /// Executes a data operation if a condition is true and the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The current result containing data.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="operation">A function that takes the data from the current result and returns a new result with the same data type.</param>
    /// <returns>
    /// The result of the operation if the current result is successful and the condition is true;
    /// otherwise, the current result.
    /// </returns>
    public static Result<T> When<T>(this Result<T> result, bool condition, Func<T, Result<T>> operation)
    {
        return !result.Success ? result : 
                condition ? operation(result.Data) : result;
    }
    
    #endregion
    
    #region Asynchronous Methods
    
    /// <summary>
    /// Asynchronously executes the next operation only if the current result is successful.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="next">An asynchronous function that returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result.
    /// </returns>
    public static async Task<Result> ThenAsync(this Result result, Func<Task<Result>> next)
    {
        return result.Success ? await next() : result;
    }
    
    /// <summary>
    /// Asynchronously executes the next operation with data from the current result, only if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the current result.</typeparam>
    /// <param name="result">The current result containing data.</param>
    /// <param name="next">An asynchronous function that takes the data from the current result and returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result converted to a non-generic result.
    /// </returns>
    public static async Task<Result> ThenAsync<T>(this Result<T> result, Func<T, Task<Result>> next)
    {
        return result.Success ? await next(result.Data) : result.AsResult();
    }
    
    /// <summary>
    /// Asynchronously executes the next operation with data from the current result and produces new data, only if the current result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of data in the current result.</typeparam>
    /// <typeparam name="TOut">The type of data in the next result.</typeparam>
    /// <param name="result">The current result containing data.</param>
    /// <param name="next">An asynchronous function that takes the data from the current result and returns a new result with different data.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, a failed result with the same error details as the current result.
    /// </returns>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next)
    {
        return result.Success ? await next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
    }
    
    /// <summary>
    /// Asynchronously executes the next operation only if the current result is successful.
    /// </summary>
    /// <param name="resultTask">A task that returns the current result.</param>
    /// <param name="next">An asynchronous function that returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result.
    /// </returns>
    public static async Task<Result> ThenAsync(this Task<Result> resultTask, Func<Task<Result>> next)
    {
        var result = await resultTask;
        return result.Success ? await next() : result;
    }
    
    /// <summary>
    /// Asynchronously executes the next operation with data from the current result, only if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the current result.</typeparam>
    /// <param name="resultTask">A task that returns the current result containing data.</param>
    /// <param name="next">An asynchronous function that takes the data from the current result and returns the next result.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, the current failed result converted to a non-generic result.
    /// </returns>
    public static async Task<Result> ThenAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<Result>> next)
    {
        var result = await resultTask;
        return result.Success ? await next(result.Data) : result.AsResult();
    }
    
    /// <summary>
    /// Asynchronously executes the next operation with data from the current result and produces new data, only if the current result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of data in the current result.</typeparam>
    /// <typeparam name="TOut">The type of data in the next result.</typeparam>
    /// <param name="resultTask">A task that returns the current result containing data.</param>
    /// <param name="next">An asynchronous function that takes the data from the current result and returns a new result with different data.</param>
    /// <returns>
    /// The result of the next operation if the current result is successful;
    /// otherwise, a failed result with the same error details as the current result.
    /// </returns>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return result.Success ? await next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
    }
    
    #endregion
}
