using System;
using System.Collections.Generic;
using System.Linq;

namespace Tethys.Results
{
    /// <summary>
    /// Represents the result of an operation that returns data of type <typeparamref name="T"/>.
    /// This class is immutable and thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>

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

}

