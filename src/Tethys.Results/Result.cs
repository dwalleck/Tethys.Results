using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tethys.Results
{
    /// <summary>
    /// Represents the result of an operation without returning data.
    /// This class is immutable and thread-safe.
    /// </summary>
    public sealed class Result : IResult, IEquatable<Result>
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

        /// <summary>
        /// Determines whether the specified <see cref="Result"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Result"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="Result"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Result other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Success == other.Success &&
                   Message == other.Message &&
                   EqualityComparer<Exception>.Default.Equals(Exception, other.Exception);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Result);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Success.GetHashCode();
                hashCode = (hashCode * 397) ^ (Message?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Exception?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="Result"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result"/> to compare.</param>
        /// <param name="right">The second <see cref="Result"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Result"/> instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Result left, Result right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Result"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result"/> to compare.</param>
        /// <param name="right">The second <see cref="Result"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Result"/> instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Result left, Result right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Pattern matches on the result, executing the appropriate function based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        /// <returns>The value returned by either the onSuccess or onFailure function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either onSuccess or onFailure is null.</exception>
        public TResult Match<TResult>(
            Func<TResult> onSuccess,
            Func<Exception, TResult> onFailure)
        {
            if (onSuccess == null)
            {
                throw new ArgumentNullException(nameof(onSuccess), "onSuccess function cannot be null");
            }

            if (onFailure == null)
            {
                throw new ArgumentNullException(nameof(onFailure), "onFailure function cannot be null");
            }

            return Success ? onSuccess() : onFailure(Exception);
        }

        /// <summary>
        /// Asynchronously pattern matches on the result, executing the appropriate function based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The async function to execute if the result is successful.</param>
        /// <param name="onFailure">The async function to execute if the result is a failure.</param>
        /// <returns>A task representing the asynchronous operation that returns the value from either the onSuccess or onFailure function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either onSuccess or onFailure is null.</exception>
        public async Task<TResult> MatchAsync<TResult>(
            Func<Task<TResult>> onSuccess,
            Func<Exception, Task<TResult>> onFailure)
        {
            if (onSuccess == null)
            {
                throw new ArgumentNullException(nameof(onSuccess), "onSuccess function cannot be null");
            }

            if (onFailure == null)
            {
                throw new ArgumentNullException(nameof(onFailure), "onFailure function cannot be null");
            }

            return Success ? await onSuccess() : await onFailure(Exception);
        }

        /// <summary>
        /// Transforms the exception of a failed result using the provided mapping function.
        /// </summary>
        /// <param name="mapper">The function to transform the exception.</param>
        /// <returns>A new Result with the transformed exception if this is a failure, or the same successful result if this is a success.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public Result MapError(Func<Exception, Exception> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            return Success ? this : new Result(false, Message, mapper(Exception));
        }

        /// <summary>
        /// Asynchronously transforms the exception of a failed result using the provided mapping function.
        /// </summary>
        /// <param name="mapper">The async function to transform the exception.</param>
        /// <returns>A task representing the asynchronous operation that returns a new Result with the transformed exception if this is a failure, or the same successful result if this is a success.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public async Task<Result> MapErrorAsync(Func<Exception, Task<Exception>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            return Success ? this : new Result(false, Message, await mapper(Exception));
        }

        /// <summary>
        /// Executes an action and returns a Result indicating success or failure.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A successful Result if the action completes without throwing an exception; otherwise, a failed Result containing the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public static Result Try(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "action cannot be null");
            }

            try
            {
                action();
                return Ok();
            }
            catch (Exception ex)
            {
                return Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Executes an action and returns a Result indicating success or failure with custom messages.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="successMessage">The message to use when the action succeeds.</param>
        /// <param name="errorMessageFormat">Optional format string for the error message. If null, the exception message is used.</param>
        /// <returns>A Result indicating success or failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action or successMessage is null.</exception>
        public static Result Try(Action action, string successMessage, string errorMessageFormat = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "action cannot be null");
            }

            if (successMessage == null)
            {
                throw new ArgumentNullException(nameof(successMessage), "successMessage cannot be null");
            }

            try
            {
                action();
                return Ok(successMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageFormat != null 
                    ? string.Format(errorMessageFormat, ex.Message)
                    : ex.Message;
                return Fail(errorMessage, ex);
            }
        }

        /// <summary>
        /// Asynchronously executes an action and returns a Result indicating success or failure.
        /// </summary>
        /// <param name="asyncAction">The async action to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns a successful Result if the action completes without throwing an exception; otherwise, a failed Result containing the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncAction is null.</exception>
        public static async Task<Result> TryAsync(Func<Task> asyncAction)
        {
            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction), "asyncAction cannot be null");
            }

            try
            {
                await asyncAction();
                return Ok();
            }
            catch (Exception ex)
            {
                return Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously executes an action and returns a Result indicating success or failure with custom messages.
        /// </summary>
        /// <param name="asyncAction">The async action to execute.</param>
        /// <param name="successMessage">The message to use when the action succeeds.</param>
        /// <param name="errorMessageFormat">Optional format string for the error message. If null, the exception message is used.</param>
        /// <returns>A task representing the asynchronous operation that returns a Result indicating success or failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncAction or successMessage is null.</exception>
        public static async Task<Result> TryAsync(Func<Task> asyncAction, string successMessage, string errorMessageFormat = null)
        {
            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction), "asyncAction cannot be null");
            }

            if (successMessage == null)
            {
                throw new ArgumentNullException(nameof(successMessage), "successMessage cannot be null");
            }

            try
            {
                await asyncAction();
                return Ok(successMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageFormat != null 
                    ? string.Format(errorMessageFormat, ex.Message)
                    : ex.Message;
                return Fail(errorMessage, ex);
            }
        }
    }

}

