using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tethys.Results
{
    /// <summary>
    /// Represents the result of an operation that returns either a value of type
    /// <typeparamref name="TValue"/> on success, or an error of type
    /// <typeparamref name="TError"/> on failure.
    /// This class is immutable and thread-safe.
    /// </summary>
    /// <typeparam name="TValue">The type of value returned on success.</typeparam>
    /// <typeparam name="TError">The type of error returned on failure.</typeparam>
    public sealed class Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
        public bool Success { get; }

        private readonly TValue _value;
        private readonly TError _error;

        private Result(bool success, TValue value, TError error)
        {
            Success = success;
            _value = value;
            _error = error;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A successful result containing the value.</returns>
        public static Result<TValue, TError> Ok(TValue value)
        {
            return new Result<TValue, TError>(true, value, default);
        }

        /// <summary>
        /// Gets the success value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessing Value on a failed result.
        /// </exception>
        /// <value>The success value when <see cref="Success"/> is <c>true</c>.</value>
        public TValue Value
        {
            get
            {
                if (!Success)
                {
                    throw new InvalidOperationException(
                        "Cannot access Value on a failed result. Check Success first or use Match().");
                }
                return _value;
            }
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A failed result containing the error.</returns>
        public static Result<TValue, TError> Fail(TError error)
        {
            return new Result<TValue, TError>(false, default, error);
        }

        /// <summary>
        /// Gets the error value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessing Error on a successful result.
        /// </exception>
        /// <value>The error value when <see cref="Success"/> is <c>false</c>.</value>
        public TError Error
        {
            get
            {
                if (Success)
                {
                    throw new InvalidOperationException(
                        "Cannot access Error on a successful result. Check Success first or use Match().");
                }
                return _error;
            }
        }

        /// <summary>
        /// Pattern matches on the result, executing the appropriate function
        /// based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of value returned by both functions.</typeparam>
        /// <param name="onSuccess">Function to execute if successful, receives the value.</param>
        /// <param name="onFailure">Function to execute if failed, receives the error.</param>
        /// <returns>The value returned by the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<TError, TResult> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return Success ? onSuccess(_value) : onFailure(_error);
        }

        /// <summary>
        /// Asynchronously pattern matches on the result, executing the appropriate function
        /// based on success or failure.
        /// </summary>
        /// <typeparam name="TResult">The type of value returned by both functions.</typeparam>
        /// <param name="onSuccess">Async function to execute if successful, receives the value.</param>
        /// <param name="onFailure">Async function to execute if failed, receives the error.</param>
        /// <returns>A task containing the value returned by the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
        public async Task<TResult> MatchAsync<TResult>(
            Func<TValue, Task<TResult>> onSuccess,
            Func<TError, Task<TResult>> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return Success
                ? await onSuccess(_value).ConfigureAwait(false)
                : await onFailure(_error).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether the specified result is equal to this instance.
        /// </summary>
        /// <param name="other">The result to compare with this instance.</param>
        /// <returns>true if the results are equal; otherwise, false.</returns>
        public bool Equals(Result<TValue, TError> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Success != other.Success)
                return false;

            return Success
                ? EqualityComparer<TValue>.Default.Equals(_value, other._value)
                : EqualityComparer<TError>.Default.Equals(_error, other._error);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Result<TValue, TError>);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Success.GetHashCode();
                hashCode = (hashCode * 397) ^ (Success
                    ? (_value == null ? 0 : EqualityComparer<TValue>.Default.GetHashCode(_value))
                    : (_error == null ? 0 : EqualityComparer<TError>.Default.GetHashCode(_error)));
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string representation of this result.
        /// </summary>
        /// <returns>A string indicating success or failure with the contained value or error.</returns>
        public override string ToString()
        {
            return Success
                ? $"Success({_value})"
                : $"Failure({_error})";
        }

        /// <summary>
        /// Determines whether two results are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result{TValue, TError}"/> to compare.</param>
        /// <param name="right">The second <see cref="Result{TValue, TError}"/> to compare.</param>
        /// <returns><c>true</c> if the two results are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two results are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Result{TValue, TError}"/> to compare.</param>
        /// <param name="right">The second <see cref="Result{TValue, TError}"/> to compare.</param>
        /// <returns><c>true</c> if the two results are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right)
        {
            return !(left == right);
        }
    }
}
