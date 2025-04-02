using System;
using System.Collections.Generic;
using System.Linq;

namespace Tethys.Results
{
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
}
