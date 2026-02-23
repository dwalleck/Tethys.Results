using System;
using System.Threading.Tasks;

namespace Tethys.Results
{
    /// <summary>
    /// Provides extension methods for chaining and transforming
    /// <see cref="Result{TValue, TError}"/> instances.
    /// </summary>
    public static class TypedResultExtensions
    {
        /// <summary>
        /// Executes the next operation only if the current result is successful,
        /// preserving the error type through the chain. The value type may change.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the new success value (may be the same as <typeparamref name="TValue"/>).</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="next">A function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.FlatMap{TNew}(Func{T, Result{TNew}})"/>, this method does not catch
        /// exceptions thrown by the next function. <see cref="Result{TValue, TError}"/> uses typed domain
        /// errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public static Result<TNewValue, TError> Then<TValue, TNewValue, TError>(
            this Result<TValue, TError> result,
            Func<TValue, Result<TNewValue, TError>> next)
        {
            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return result.Success
                ? next(result.Value)
                : Result<TNewValue, TError>.Fail(result.Error);
        }

        /// <summary>
        /// Conditionally executes an operation if the current result is successful
        /// and the specified condition is true.
        /// </summary>
        /// <typeparam name="TValue">The type of the success value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="operation">A function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the operation if the current result is successful and the condition is true;
        /// otherwise, the current result.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
        public static Result<TValue, TError> When<TValue, TError>(
            this Result<TValue, TError> result,
            bool condition,
            Func<TValue, Result<TValue, TError>> operation)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (!result.Success)
            {
                return result;
            }

            return condition ? operation(result.Value) : result;
        }

        /// <summary>
        /// Transforms the success value without requiring the caller to wrap the
        /// return value in a <see cref="Result{TValue, TError}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the transformed value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="mapper">A function that transforms the success value.</param>
        /// <returns>
        /// A successful result with the transformed value if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.Map{TNew}(Func{T, TNew})"/>, this method does not catch
        /// exceptions thrown by the mapper. <see cref="Result{TValue, TError}"/> uses typed domain
        /// errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
        public static Result<TNewValue, TError> Map<TValue, TNewValue, TError>(
            this Result<TValue, TError> result,
            Func<TValue, TNewValue> mapper)
        {
            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return result.Success
                ? Result<TNewValue, TError>.Ok(mapper(result.Value))
                : Result<TNewValue, TError>.Fail(result.Error);
        }

        /// <summary>
        /// Asynchronously executes the next operation only if the current result is successful,
        /// preserving the error type through the chain. The value type may change.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the new success value (may be the same as <typeparamref name="TValue"/>).</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="next">An async function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.FlatMapAsync{TNew}(Func{T, Task{Result{TNew}}})"/>, this method does not
        /// catch exceptions thrown by the next function. <see cref="Result{TValue, TError}"/> uses typed domain
        /// errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public static async Task<Result<TNewValue, TError>> ThenAsync<TValue, TNewValue, TError>(
            this Result<TValue, TError> result,
            Func<TValue, Task<Result<TNewValue, TError>>> next)
        {
            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return result.Success
                ? await next(result.Value).ConfigureAwait(false)
                : Result<TNewValue, TError>.Fail(result.Error);
        }

        /// <summary>
        /// Asynchronously executes the next operation on a task-wrapped result,
        /// preserving the error type through the chain. The value type may change.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the new success value (may be the same as <typeparamref name="TValue"/>).</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="resultTask">A task that returns the current result.</param>
        /// <param name="next">An async function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.FlatMapAsync{TNew}(Func{T, Task{Result{TNew}}})"/>, this method does not
        /// catch exceptions thrown by the next function. <see cref="Result{TValue, TError}"/> uses typed domain
        /// errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resultTask"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public static async Task<Result<TNewValue, TError>> ThenAsync<TValue, TNewValue, TError>(
            this Task<Result<TValue, TError>> resultTask,
            Func<TValue, Task<Result<TNewValue, TError>>> next)
        {
            if (resultTask is null)
            {
                throw new ArgumentNullException(nameof(resultTask));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var result = await resultTask.ConfigureAwait(false);
            return result.Success
                ? await next(result.Value).ConfigureAwait(false)
                : Result<TNewValue, TError>.Fail(result.Error);
        }

        /// <summary>
        /// Asynchronously and conditionally executes an operation if the current result
        /// is successful and the specified condition is true.
        /// </summary>
        /// <typeparam name="TValue">The type of the success value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="operation">An async function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the operation if the current result is successful and the condition is true;
        /// otherwise, the current result.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
        public static async Task<Result<TValue, TError>> WhenAsync<TValue, TError>(
            this Result<TValue, TError> result,
            bool condition,
            Func<TValue, Task<Result<TValue, TError>>> operation)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (!result.Success)
            {
                return result;
            }

            return condition
                ? await operation(result.Value).ConfigureAwait(false)
                : result;
        }

        /// <summary>
        /// Asynchronously and conditionally executes an operation on a task-wrapped result,
        /// only if the result is successful and the specified condition is true.
        /// </summary>
        /// <typeparam name="TValue">The type of the success value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="resultTask">A task that returns the current result.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="operation">An async function that takes the success value and returns a new result.</param>
        /// <returns>
        /// The result of the operation if the current result is successful and the condition is true;
        /// otherwise, the current result.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resultTask"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
        public static async Task<Result<TValue, TError>> WhenAsync<TValue, TError>(
            this Task<Result<TValue, TError>> resultTask,
            bool condition,
            Func<TValue, Task<Result<TValue, TError>>> operation)
        {
            if (resultTask is null)
            {
                throw new ArgumentNullException(nameof(resultTask));
            }

            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var result = await resultTask.ConfigureAwait(false);
            if (!result.Success)
            {
                return result;
            }

            return condition
                ? await operation(result.Value).ConfigureAwait(false)
                : result;
        }

        /// <summary>
        /// Asynchronously transforms the success value without requiring the caller
        /// to wrap the return value in a <see cref="Result{TValue, TError}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the transformed value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="mapper">An async function that transforms the success value.</param>
        /// <returns>
        /// A successful result with the transformed value if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.MapAsync{TNew}(Func{T, Task{TNew}})"/>, this method does not
        /// catch exceptions thrown by the mapper. <see cref="Result{TValue, TError}"/> uses typed
        /// domain errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
        public static async Task<Result<TNewValue, TError>> MapAsync<TValue, TNewValue, TError>(
            this Result<TValue, TError> result,
            Func<TValue, Task<TNewValue>> mapper)
        {
            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (!result.Success)
            {
                return Result<TNewValue, TError>.Fail(result.Error);
            }

            var newValue = await mapper(result.Value).ConfigureAwait(false);
            return Result<TNewValue, TError>.Ok(newValue);
        }

        /// <summary>
        /// Asynchronously transforms the success value on a task-wrapped result without
        /// requiring the caller to wrap the return value in a <see cref="Result{TValue, TError}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the current success value.</typeparam>
        /// <typeparam name="TNewValue">The type of the transformed value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <param name="resultTask">A task that returns the current result.</param>
        /// <param name="mapper">An async function that transforms the success value.</param>
        /// <returns>
        /// A successful result with the transformed value if the current result is successful;
        /// otherwise, a failed result with the original error.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Result{T}.MapAsync{TNew}(Func{T, Task{TNew}})"/>, this method does not
        /// catch exceptions thrown by the mapper. <see cref="Result{TValue, TError}"/> uses typed
        /// domain errors rather than exceptions; unexpected exceptions should propagate to the caller.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resultTask"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
        public static async Task<Result<TNewValue, TError>> MapAsync<TValue, TNewValue, TError>(
            this Task<Result<TValue, TError>> resultTask,
            Func<TValue, Task<TNewValue>> mapper)
        {
            if (resultTask is null)
            {
                throw new ArgumentNullException(nameof(resultTask));
            }

            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            var result = await resultTask.ConfigureAwait(false);
            if (!result.Success)
            {
                return Result<TNewValue, TError>.Fail(result.Error);
            }

            var newValue = await mapper(result.Value).ConfigureAwait(false);
            return Result<TNewValue, TError>.Ok(newValue);
        }
    }
}
