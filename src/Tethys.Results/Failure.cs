using System;
using System.Collections.Generic;

namespace Tethys.Results
{
    /// <summary>
    /// A lightweight wrapper that tags a value as a failure, enabling unambiguous
    /// implicit conversion to <see cref="Result{TValue, TError}"/>.
    /// This is a value type with no heap allocation overhead.
    /// </summary>
    /// <typeparam name="T">The type of the error value.</typeparam>
    public readonly struct Failure<T> : IEquatable<Failure<T>>
    {
        /// <summary>
        /// Gets the error value.
        /// </summary>
        public T Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Failure{T}"/> struct.
        /// </summary>
        /// <param name="error">The error value to wrap.</param>
        public Failure(T error)
        {
            Error = error;
        }

        /// <inheritdoc />
        public bool Equals(Failure<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Error, other.Error);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Failure<T> other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Error == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Error);
        }

        /// <summary>
        /// Returns a string representation of this failure wrapper.
        /// </summary>
        /// <returns>A string in the format "Failure(error)".</returns>
        public override string ToString()
        {
            return Error == null ? "Failure(null)" : $"Failure({Error})";
        }

        /// <summary>
        /// Determines whether two <see cref="Failure{T}"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if both instances wrap equal error values; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Failure<T> left, Failure<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Failure{T}"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns><c>true</c> if the instances wrap different error values; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Failure<T> left, Failure<T> right)
        {
            return !left.Equals(right);
        }
    }
}
