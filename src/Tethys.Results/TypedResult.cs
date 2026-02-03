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
        /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
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

        // Equality stub - will implement fully later
        public bool Equals(Result<TValue, TError> other) => false;
    }
}
