using System;

namespace Tethys.Results
{
    public interface IResult
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
        bool Success { get; }

        /// <summary>
        /// Gets a descriptive message about the operation result.
        /// </summary>
        /// <value>A success message when <see cref="Success"/> is <c>true</c>, or an error message when <see cref="Success"/> is <c>false</c>.</value>
        string Message { get; }

        /// <summary>
        /// Gets the exception that caused the operation to fail, if any.
        /// </summary>
        /// <value>The exception that caused the failure, or <c>null</c> if the operation succeeded or failed without an exception.</value>
        Exception Exception { get; }
    }
}
