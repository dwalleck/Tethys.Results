using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tethys.Results
{
    /// <summary>
    /// Represents the result of an operation that returns data of type <typeparamref name="T"/>.
    /// This class is immutable and thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    public sealed class Result<T> : IResult, IEquatable<Result<T>>
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

        /// <summary>
        /// Determines whether the specified <see cref="Result{T}"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Result{T}"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="Result{T}"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Result<T> other)
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
                   EqualityComparer<T>.Default.Equals(Data, other.Data) &&
                   EqualityComparer<Exception>.Default.Equals(Exception, other.Exception);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Result<T>);
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
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Data);
                hashCode = (hashCode * 397) ^ (Exception?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="Result{T}"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Result{T}"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Result{T}"/> instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Result<T> left, Result<T> right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Result{T}"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Result{T}"/> to compare.</param>
        /// <returns><c>true</c> if the two <see cref="Result{T}"/> instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Result<T> left, Result<T> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Pattern matches on the result, executing the appropriate function based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful, receiving the data value.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving the exception.</param>
        /// <returns>The value returned by either the onSuccess or onFailure function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either onSuccess or onFailure is null.</exception>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
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

            return Success ? onSuccess(Data) : onFailure(Exception);
        }

        /// <summary>
        /// Asynchronously pattern matches on the result, executing the appropriate function based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The async function to execute if the result is successful, receiving the data value.</param>
        /// <param name="onFailure">The async function to execute if the result is a failure, receiving the exception.</param>
        /// <returns>A task representing the asynchronous operation that returns the value from either the onSuccess or onFailure function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either onSuccess or onFailure is null.</exception>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onSuccess,
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

            return Success ? await onSuccess(Data) : await onFailure(Exception);
        }

        /// <summary>
        /// Transforms the value of a successful result using the provided mapping function.
        /// </summary>
        /// <typeparam name="TNew">The type of the transformed value.</typeparam>
        /// <param name="mapper">The function to transform the value.</param>
        /// <returns>A new Result containing the transformed value if this is a success, or a failed result with the same error if this is a failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            if (!Success)
            {
                return new Result<TNew>(false, Message, default, Exception);
            }

            try
            {
                return Result<TNew>.Ok(mapper(Data), Message);
            }
            catch (Exception ex)
            {
                return Result<TNew>.Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously transforms the value of a successful result using the provided mapping function.
        /// </summary>
        /// <typeparam name="TNew">The type of the transformed value.</typeparam>
        /// <param name="mapper">The async function to transform the value.</param>
        /// <returns>A task representing the asynchronous operation that returns a new Result containing the transformed value if this is a success, or a failed result with the same error if this is a failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            if (!Success)
            {
                return new Result<TNew>(false, Message, default, Exception);
            }

            try
            {
                var newValue = await mapper(Data);
                return Result<TNew>.Ok(newValue, Message);
            }
            catch (Exception ex)
            {
                return Result<TNew>.Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Chains operations that return Results, flattening the nested Result structure.
        /// </summary>
        /// <typeparam name="TNew">The type of the value in the new Result.</typeparam>
        /// <param name="mapper">The function that takes the current value and returns a new Result.</param>
        /// <returns>The Result returned by the mapper function if this is a success, or a failed result with the same error if this is a failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public Result<TNew> FlatMap<TNew>(Func<T, Result<TNew>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            if (!Success)
            {
                return new Result<TNew>(false, Message, default, Exception);
            }

            try
            {
                return mapper(Data);
            }
            catch (Exception ex)
            {
                return Result<TNew>.Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously chains operations that return Results, flattening the nested Result structure.
        /// </summary>
        /// <typeparam name="TNew">The type of the value in the new Result.</typeparam>
        /// <param name="mapper">The async function that takes the current value and returns a new Result.</param>
        /// <returns>A task representing the asynchronous operation that returns the Result from the mapper function if this is a success, or a failed result with the same error if this is a failure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public async Task<Result<TNew>> FlatMapAsync<TNew>(Func<T, Task<Result<TNew>>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            if (!Success)
            {
                return new Result<TNew>(false, Message, default, Exception);
            }

            try
            {
                return await mapper(Data);
            }
            catch (Exception ex)
            {
                return Result<TNew>.Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Transforms the exception of a failed result using the provided mapping function.
        /// </summary>
        /// <param name="mapper">The function to transform the exception.</param>
        /// <returns>A new Result with the transformed exception if this is a failure, or the same successful result if this is a success.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public Result<T> MapError(Func<Exception, Exception> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            return Success ? this : new Result<T>(false, Message, Data, mapper(Exception));
        }

        /// <summary>
        /// Asynchronously transforms the exception of a failed result using the provided mapping function.
        /// </summary>
        /// <param name="mapper">The async function to transform the exception.</param>
        /// <returns>A task representing the asynchronous operation that returns a new Result with the transformed exception if this is a failure, or the same successful result if this is a success.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        public async Task<Result<T>> MapErrorAsync(Func<Exception, Task<Exception>> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper), "mapper function cannot be null");
            }

            return Success ? this : new Result<T>(false, Message, Data, await mapper(Exception));
        }

        /// <summary>
        /// Executes a function and returns a Result containing the value or an exception.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>A successful Result containing the function's return value if it completes without throwing an exception; otherwise, a failed Result containing the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
        public static Result<T> Try(Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func), "func cannot be null");
            }

            try
            {
                var value = func();
                return Ok(value);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Executes a function and returns a Result containing the value or an exception with custom messages.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="successMessage">The message to use when the function succeeds.</param>
        /// <param name="errorMessageFormat">Optional format string for the error message. If null, the exception message is used.</param>
        /// <returns>A Result containing the value or an exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when func or successMessage is null.</exception>
        public static Result<T> Try(Func<T> func, string successMessage, string errorMessageFormat = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func), "func cannot be null");
            }

            if (successMessage == null)
            {
                throw new ArgumentNullException(nameof(successMessage), "successMessage cannot be null");
            }

            try
            {
                var value = func();
                return Ok(value, successMessage);
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
        /// Asynchronously executes a function and returns a Result containing the value or an exception.
        /// </summary>
        /// <param name="asyncFunc">The async function to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns a successful Result containing the function's return value if it completes without throwing an exception; otherwise, a failed Result containing the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncFunc is null.</exception>
        public static async Task<Result<T>> TryAsync(Func<Task<T>> asyncFunc)
        {
            if (asyncFunc == null)
            {
                throw new ArgumentNullException(nameof(asyncFunc), "asyncFunc cannot be null");
            }

            try
            {
                var value = await asyncFunc();
                return Ok(value);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously executes a function and returns a Result containing the value or an exception with custom messages.
        /// </summary>
        /// <param name="asyncFunc">The async function to execute.</param>
        /// <param name="successMessage">The message to use when the function succeeds.</param>
        /// <param name="errorMessageFormat">Optional format string for the error message. If null, the exception message is used.</param>
        /// <returns>A task representing the asynchronous operation that returns a Result containing the value or an exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when asyncFunc or successMessage is null.</exception>
        public static async Task<Result<T>> TryAsync(Func<Task<T>> asyncFunc, string successMessage, string errorMessageFormat = null)
        {
            if (asyncFunc == null)
            {
                throw new ArgumentNullException(nameof(asyncFunc), "asyncFunc cannot be null");
            }

            if (successMessage == null)
            {
                throw new ArgumentNullException(nameof(successMessage), "successMessage cannot be null");
            }

            try
            {
                var value = await asyncFunc();
                return Ok(value, successMessage);
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

