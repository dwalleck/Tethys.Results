using System;
using System.Threading.Tasks;

namespace Tethys.Results
{

    /// <summary>
    /// Provides extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> instances.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Executes the next operation only if the current result is successful.
        /// </summary>
        /// <param name="result">The current result.</param>
        /// <param name="next">A function that returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result.
        /// </returns>
        public static Result Then(this Result result, Func<Result> next)
        {
            return result.Success ? next() : result;
        }

        /// <summary>
        /// Executes the next operation with data from the current result, only if the current result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the current result.</typeparam>
        /// <param name="result">The current result containing data.</param>
        /// <param name="next">A function that takes the data from the current result and returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result converted to a non-generic result.
        /// </returns>
        public static Result Then<T>(this Result<T> result, Func<T, Result> next)
        {
            return result.Success ? next(result.Data) : result.AsResult();
        }

        /// <summary>
        /// Executes the next operation with data from the current result and produces new data, only if the current result is successful.
        /// </summary>
        /// <typeparam name="TIn">The type of data in the current result.</typeparam>
        /// <typeparam name="TOut">The type of data in the next result.</typeparam>
        /// <param name="result">The current result containing data.</param>
        /// <param name="next">A function that takes the data from the current result and returns a new result with different data.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the same error details as the current result.
        /// </returns>
        public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next)
        {
            return result.Success ? next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
        }

        /// <summary>
        /// Executes an operation if a condition is true and the current result is successful.
        /// </summary>
        /// <param name="result">The current result.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="operation">A function that returns the next result.</param>
        /// <returns>
        /// The result of the operation if the current result is successful and the condition is true;
        /// otherwise, the current result.
        /// </returns>
        public static Result When(this Result result, bool condition, Func<Result> operation)
        {
            return !result.Success ? result :
                    condition ? operation() : result;
        }

        /// <summary>
        /// Executes a data operation if a condition is true and the current result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The current result containing data.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="operation">A function that takes the data from the current result and returns a new result with the same data type.</param>
        /// <returns>
        /// The result of the operation if the current result is successful and the condition is true;
        /// otherwise, the current result.
        /// </returns>
        public static Result<T> When<T>(this Result<T> result, bool condition, Func<T, Result<T>> operation)
        {
            return !result.Success ? result :
                    condition ? operation(result.Data) : result;
        }

        /// <summary>
        /// Asynchronously executes the next operation only if the current result is successful.
        /// </summary>
        /// <param name="result">The current result.</param>
        /// <param name="next">An asynchronous function that returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result.
        /// </returns>
        public static async Task<Result> ThenAsync(this Result result, Func<Task<Result>> next)
        {
            return result.Success ? await next() : result;
        }

        /// <summary>
        /// Asynchronously executes the next operation with data from the current result, only if the current result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the current result.</typeparam>
        /// <param name="result">The current result containing data.</param>
        /// <param name="next">An asynchronous function that takes the data from the current result and returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result converted to a non-generic result.
        /// </returns>
        public static async Task<Result> ThenAsync<T>(this Result<T> result, Func<T, Task<Result>> next)
        {
            return result.Success ? await next(result.Data) : result.AsResult();
        }

        /// <summary>
        /// Asynchronously executes the next operation with data from the current result and produces new data, only if the current result is successful.
        /// </summary>
        /// <typeparam name="TIn">The type of data in the current result.</typeparam>
        /// <typeparam name="TOut">The type of data in the next result.</typeparam>
        /// <param name="result">The current result containing data.</param>
        /// <param name="next">An asynchronous function that takes the data from the current result and returns a new result with different data.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the same error details as the current result.
        /// </returns>
        public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next)
        {
            return result.Success ? await next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
        }

        /// <summary>
        /// Asynchronously executes the next operation only if the current result is successful.
        /// </summary>
        /// <param name="resultTask">A task that returns the current result.</param>
        /// <param name="next">An asynchronous function that returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result.
        /// </returns>
        public static async Task<Result> ThenAsync(this Task<Result> resultTask, Func<Task<Result>> next)
        {
            var result = await resultTask;
            return result.Success ? await next() : result;
        }

        /// <summary>
        /// Asynchronously executes the next operation with data from the current result, only if the current result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the current result.</typeparam>
        /// <param name="resultTask">A task that returns the current result containing data.</param>
        /// <param name="next">An asynchronous function that takes the data from the current result and returns the next result.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, the current failed result converted to a non-generic result.
        /// </returns>
        public static async Task<Result> ThenAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<Result>> next)
        {
            var result = await resultTask;
            return result.Success ? await next(result.Data) : result.AsResult();
        }

        /// <summary>
        /// Asynchronously executes the next operation with data from the current result and produces new data, only if the current result is successful.
        /// </summary>
        /// <typeparam name="TIn">The type of data in the current result.</typeparam>
        /// <typeparam name="TOut">The type of data in the next result.</typeparam>
        /// <param name="resultTask">A task that returns the current result containing data.</param>
        /// <param name="next">An asynchronous function that takes the data from the current result and returns a new result with different data.</param>
        /// <returns>
        /// The result of the next operation if the current result is successful;
        /// otherwise, a failed result with the same error details as the current result.
        /// </returns>
        public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> next)
        {
            var result = await resultTask;
            return result.Success ? await next(result.Data) : Result<TOut>.Fail(result.Message, result.Exception);
        }

        #region OnSuccess Callback Methods

        /// <summary>
        /// Executes the specified callback if the result is successful.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The callback to execute if the result is successful.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result OnSuccess(this Result result, Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (result.Success)
            {
                callback();
            }

            return result;
        }

        /// <summary>
        /// Executes the specified callback with the result data if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The callback to execute with the result data if the result is successful.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (result.Success)
            {
                callback(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback if the result is successful.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The async callback to execute if the result is successful.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result> OnSuccessAsync(this Result result, Func<Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (result.Success)
            {
                await callback().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback with the result data if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The async callback to execute with the result data if the result is successful.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<T, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (result.Success)
            {
                await callback(result.Data).ConfigureAwait(false);
            }

            return result;
        }

        #endregion

        #region OnFailure Callback Methods

        /// <summary>
        /// Executes the specified callback if the result is a failure.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The callback to execute with the exception if the result is a failure.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result OnFailure(this Result result, Action<Exception> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!result.Success)
            {
                callback(result.Exception);
            }

            return result;
        }

        /// <summary>
        /// Executes the specified callback if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The callback to execute with the exception if the result is a failure.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<Exception> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!result.Success)
            {
                callback(result.Exception);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback if the result is a failure.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The async callback to execute with the exception if the result is a failure.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result> OnFailureAsync(this Result result, Func<Exception, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!result.Success)
            {
                await callback(result.Exception).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="callback">The async callback to execute with the exception if the result is a failure.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Exception, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!result.Success)
            {
                await callback(result.Exception).ConfigureAwait(false);
            }

            return result;
        }

        #endregion

        #region OnBoth Callback Methods

        /// <summary>
        /// Executes the specified callback regardless of whether the result is successful or failed.
        /// This is useful for logging or cleanup operations.
        /// </summary>
        /// <param name="result">The result to pass to the callback.</param>
        /// <param name="callback">The callback to execute with the result.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result OnBoth(this Result result, Action<Result> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            callback(result);
            return result;
        }

        /// <summary>
        /// Executes the specified callback regardless of whether the result is successful or failed.
        /// This is useful for logging or cleanup operations.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to pass to the callback.</param>
        /// <param name="callback">The callback to execute with the result.</param>
        /// <returns>The original result, allowing for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static Result<T> OnBoth<T>(this Result<T> result, Action<Result<T>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            callback(result);
            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback regardless of whether the result is successful or failed.
        /// This is useful for logging or cleanup operations.
        /// </summary>
        /// <param name="result">The result to pass to the callback.</param>
        /// <param name="callback">The async callback to execute with the result.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result> OnBothAsync(this Result result, Func<Result, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            await callback(result).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Asynchronously executes the specified callback regardless of whether the result is successful or failed.
        /// This is useful for logging or cleanup operations.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to pass to the callback.</param>
        /// <param name="callback">The async callback to execute with the result.</param>
        /// <returns>A task that represents the asynchronous operation, containing the original result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public static async Task<Result<T>> OnBothAsync<T>(this Result<T> result, Func<Result<T>, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            await callback(result).ConfigureAwait(false);
            return result;
        }

        #endregion
    }

}
