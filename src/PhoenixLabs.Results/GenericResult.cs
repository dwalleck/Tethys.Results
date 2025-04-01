using System;
using System.Collections.Generic;
using System.Linq;

namespace PhoenixLabs.Results
{
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

}

