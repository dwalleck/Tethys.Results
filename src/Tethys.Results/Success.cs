using System;
using System.Collections.Generic;

namespace Tethys.Results
{
    /// <summary>
    /// A lightweight wrapper that tags a value as a success, enabling unambiguous
    /// implicit conversion to <see cref="Result{TValue, TError}"/>.
    /// This is a value type with no heap allocation overhead.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    public readonly struct Success<T> : IEquatable<Success<T>>
    {
        /// <summary>
        /// Gets the success value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Success{T}"/> struct.
        /// </summary>
        /// <param name="value">The success value to wrap.</param>
        public Success(T value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(Success<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Success<T> other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value);
        }

        /// <summary>
        /// Returns a string representation of this success wrapper.
        /// </summary>
        /// <returns>A string in the format "Success(value)".</returns>
        public override string ToString()
        {
            return Value == null ? "Success(null)" : $"Success({Value})";
        }

        /// <summary>
        /// Determines whether two <see cref="Success{T}"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if both instances wrap equal values; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Success<T> left, Success<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Success{T}"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances wrap different values; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Success<T> left, Success<T> right)
        {
            return !left.Equals(right);
        }
    }
}
