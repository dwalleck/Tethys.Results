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

        // Equality stub - will implement fully later
        public bool Equals(Result<TValue, TError> other) => false;
    }
}
