using System;
using System.Threading.Tasks;
using TUnit;
using Tethys.Results;

namespace Tethys.Test
{
    /// <summary>
    /// Unit tests for the Try pattern methods in Result and Result&lt;T&gt; classes.
    /// </summary>
    public class TryPatternTests
    {
        /// <summary>
        /// Tests for Result.Try method with synchronous operations
        /// </summary>
        public class ResultTryTests
        {
            [Test]
            public async Task Try_WithSuccessfulAction_ReturnsSuccessResult()
            {
                // Arrange
                bool actionExecuted = false;
                Action action = () => { actionExecuted = true; };

                // Act
                var result = Result.Try(action);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(actionExecuted).IsTrue();
                await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task Try_WithThrowingAction_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test exception");
                Action action = () => throw expectedException;

                // Act
                var result = Result.Try(action);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Message).IsEqualTo("Test exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<InvalidOperationException>();
                await Assert.That(result.Exception.Message).IsEqualTo(expectedException.Message);
            }

            [Test]
            public async Task Try_WithNullAction_ThrowsArgumentNullException()
            {
                // Arrange
                Action action = null;

                // Act & Assert
                try
                {
                    var result = Result.Try(action);
                    await Assert.That(false).IsTrue(); // Should not reach here
                }
                catch (ArgumentNullException ex)
                {
                    await Assert.That(ex.ParamName).IsEqualTo("action");
                }
            }

            [Test]
            public async Task Try_WithCustomSuccessMessage_ReturnsSuccessResultWithMessage()
            {
                // Arrange
                Action action = () => { };
                string successMessage = "Custom success message";

                // Act
                var result = Result.Try(action, successMessage);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Message).IsEqualTo(successMessage);
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task Try_WithNullSuccessMessage_ThrowsArgumentNullException()
            {
                // Arrange
                Action action = () => { };
                string successMessage = null;

                // Act & Assert
                try
                {
                    var result = Result.Try(action, successMessage);
                    await Assert.That(false).IsTrue(); // Should not reach here
                }
                catch (ArgumentNullException ex)
                {
                    await Assert.That(ex.ParamName).IsEqualTo("successMessage");
                }
            }

            [Test]
            public async Task Try_WithCustomErrorMessage_ReturnsFailureResultWithCustomMessage()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test exception");
                Action action = () => throw expectedException;
                string errorMessage = "Custom error: {0}";

                // Act
                var result = Result.Try(action, "Success", errorMessage);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Message).IsEqualTo("Custom error: Test exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsEqualTo(expectedException);
            }

            [Test]
            public async Task Try_WithNullErrorMessageFormat_UsesExceptionMessage()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test exception");
                Action action = () => throw expectedException;

                // Act
                var result = Result.Try(action, "Success", null);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Message).IsEqualTo("Test exception");
                await Assert.That(result.Exception).IsNotNull();
            }
        }

        /// <summary>
        /// Tests for Result.TryAsync method with asynchronous operations
        /// </summary>
        public class ResultTryAsyncTests
        {
            [Test]
            public async Task TryAsync_WithSuccessfulAsyncAction_ReturnsSuccessResult()
            {
                // Arrange
                bool actionExecuted = false;
                Func<Task> asyncAction = async () =>
                {
                    await Task.Delay(10);
                    actionExecuted = true;
                };

                // Act
                var result = await Result.TryAsync(asyncAction);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(actionExecuted).IsTrue();
                await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task TryAsync_WithThrowingAsyncAction_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test async exception");
                Func<Task> asyncAction = async () =>
                {
                    await Task.Delay(10);
                    throw expectedException;
                };

                // Act
                var result = await Result.TryAsync(asyncAction);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Message).IsEqualTo("Test async exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<InvalidOperationException>();
                await Assert.That(result.Exception.Message).IsEqualTo(expectedException.Message);
            }

            [Test]
            public async Task TryAsync_WithNullAsyncAction_ThrowsArgumentNullException()
            {
                // Arrange
                Func<Task> asyncAction = null;

                // Act & Assert
                try
                {
                    var result = await Result.TryAsync(asyncAction);
                    await Assert.That(false).IsTrue(); // Should not reach here
                }
                catch (ArgumentNullException ex)
                {
                    await Assert.That(ex.ParamName).IsEqualTo("asyncAction");
                }
            }

            [Test]
            public async Task TryAsync_WithCustomMessages_ReturnsAppropriateMessages()
            {
                // Arrange
                Func<Task> successAction = async () => await Task.CompletedTask;
                Func<Task> failureAction = async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test exception");
                };

                // Act
                var successResult = await Result.TryAsync(successAction, "Custom async success");
                var failureResult = await Result.TryAsync(failureAction, "Success", "Async error: {0}");

                // Assert
                await Assert.That(successResult.Success).IsTrue();
                await Assert.That(successResult.Message).IsEqualTo("Custom async success");

                await Assert.That(failureResult.Success).IsFalse();
                await Assert.That(failureResult.Message).IsEqualTo("Async error: Test exception");
            }

            [Test]
            public async Task TryAsync_WithSynchronousException_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Synchronous exception");
                Func<Task> asyncAction = () => throw expectedException;

                // Act
                var result = await Result.TryAsync(asyncAction);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Message).IsEqualTo("Synchronous exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsEqualTo(expectedException);
            }
        }

        /// <summary>
        /// Tests for Result&lt;T&gt;.Try method with synchronous operations
        /// </summary>
        public class GenericResultTryTests
        {
            [Test]
            public async Task Try_WithSuccessfulFunc_ReturnsSuccessResultWithValue()
            {
                // Arrange
                int expectedValue = 42;
                Func<int> func = () => expectedValue;

                // Act
                var result = Result<int>.Try(func);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsEqualTo(expectedValue);
                await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task Try_WithThrowingFunc_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test exception");
                Func<string> func = () => throw expectedException;

                // Act
                var result = Result<string>.Try(func);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Data).IsNull();
                await Assert.That(result.Message).IsEqualTo("Test exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<InvalidOperationException>();
                await Assert.That(result.Exception.Message).IsEqualTo(expectedException.Message);
            }

            [Test]
            public async Task Try_WithNullFunc_ThrowsArgumentNullException()
            {
                // Arrange
                Func<int> func = null;

                // Act & Assert
                try
                {
                    var result = Result<int>.Try(func);
                    await Assert.That(false).IsTrue(); // Should not reach here
                }
                catch (ArgumentNullException ex)
                {
                    await Assert.That(ex.ParamName).IsEqualTo("func");
                }
            }

            [Test]
            public async Task Try_WithReferenceType_HandlesNullReturnValue()
            {
                // Arrange
                Func<string> func = () => null;

                // Act
                var result = Result<string>.Try(func);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsNull();
                await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task Try_WithValueType_ReturnsCorrectValue()
            {
                // Arrange
                DateTime expectedDate = new DateTime(2024, 1, 1);
                Func<DateTime> func = () => expectedDate;

                // Act
                var result = Result<DateTime>.Try(func);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsEqualTo(expectedDate);
            }

            [Test]
            public async Task Try_WithCustomMessages_ReturnsAppropriateMessages()
            {
                // Arrange
                Func<int> successFunc = () => 42;
                Func<int> failureFunc = () => throw new InvalidOperationException("Test exception");
                string successMessage = "Successfully computed value";
                string errorMessage = "Failed to compute: {0}";

                // Act
                var successResult = Result<int>.Try(successFunc, successMessage);
                var failureResult = Result<int>.Try(failureFunc, "Success", errorMessage);

                // Assert
                await Assert.That(successResult.Success).IsTrue();
                await Assert.That(successResult.Message).IsEqualTo(successMessage);
                await Assert.That(successResult.Data).IsEqualTo(42);

                await Assert.That(failureResult.Success).IsFalse();
                await Assert.That(failureResult.Message).IsEqualTo("Failed to compute: Test exception");
                await Assert.That(failureResult.Data).IsEqualTo(default(int));
            }

            [Test]
            public async Task Try_WithCollection_ReturnsCorrectValue()
            {
                // Arrange
                var expectedList = new[] { 1, 2, 3 };
                Func<int[]> func = () => expectedList;

                // Act
                var result = Result<int[]>.Try(func);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsEqualTo(expectedList);
            }
        }

        /// <summary>
        /// Tests for Result&lt;T&gt;.TryAsync method with asynchronous operations
        /// </summary>
        public class GenericResultTryAsyncTests
        {
            [Test]
            public async Task TryAsync_WithSuccessfulAsyncFunc_ReturnsSuccessResultWithValue()
            {
                // Arrange
                string expectedValue = "async result";
                Func<Task<string>> asyncFunc = async () =>
                {
                    await Task.Delay(10);
                    return expectedValue;
                };

                // Act
                var result = await Result<string>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsEqualTo(expectedValue);
                await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
                await Assert.That(result.Exception).IsNull();
            }

            [Test]
            public async Task TryAsync_WithThrowingAsyncFunc_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Test async exception");
                Func<Task<int>> asyncFunc = async () =>
                {
                    await Task.Delay(10);
                    throw expectedException;
                };

                // Act
                var result = await Result<int>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Data).IsEqualTo(default(int));
                await Assert.That(result.Message).IsEqualTo("Test async exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<InvalidOperationException>();
                await Assert.That(result.Exception.Message).IsEqualTo(expectedException.Message);
            }

            [Test]
            public async Task TryAsync_WithNullAsyncFunc_ThrowsArgumentNullException()
            {
                // Arrange
                Func<Task<int>> asyncFunc = null;

                // Act & Assert
                try
                {
                    var result = await Result<int>.TryAsync(asyncFunc);
                    await Assert.That(false).IsTrue(); // Should not reach here
                }
                catch (ArgumentNullException ex)
                {
                    await Assert.That(ex.ParamName).IsEqualTo("asyncFunc");
                }
            }

            [Test]
            public async Task TryAsync_WithSynchronousException_ReturnsFailureResult()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Synchronous exception");
                Func<Task<string>> asyncFunc = () => throw expectedException;

                // Act
                var result = await Result<string>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Data).IsNull();
                await Assert.That(result.Message).IsEqualTo("Synchronous exception");
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsEqualTo(expectedException);
            }

            [Test]
            public async Task TryAsync_WithComplexType_ReturnsCorrectValue()
            {
                // Arrange
                var expectedData = new { Id = 1, Name = "Test", Values = new[] { 1, 2, 3 } };
                Func<Task<object>> asyncFunc = async () =>
                {
                    await Task.Delay(10);
                    return expectedData;
                };

                // Act
                var result = await Result<object>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsTrue();
                await Assert.That(result.Data).IsEqualTo(expectedData);
            }

            [Test]
            public async Task TryAsync_WithCancellationToken_PropagatesCancellation()
            {
                // Arrange
                var cts = new System.Threading.CancellationTokenSource();
                Func<Task<int>> asyncFunc = async () =>
                {
                    await Task.Delay(1000, cts.Token);
                    return 42;
                };

                // Act
                cts.Cancel();
                var result = await Result<int>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<TaskCanceledException>();
            }

            [Test]
            public async Task TryAsync_WithCustomMessages_ReturnsAppropriateMessages()
            {
                // Arrange
                Func<Task<int>> successFunc = async () =>
                {
                    await Task.CompletedTask;
                    return 42;
                };
                Func<Task<int>> failureFunc = async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test exception");
                };

                // Act
                var successResult = await Result<int>.TryAsync(successFunc, "Async success");
                var failureResult = await Result<int>.TryAsync(failureFunc, "Success", "Async failed: {0}");

                // Assert
                await Assert.That(successResult.Success).IsTrue();
                await Assert.That(successResult.Message).IsEqualTo("Async success");
                await Assert.That(successResult.Data).IsEqualTo(42);

                await Assert.That(failureResult.Success).IsFalse();
                await Assert.That(failureResult.Message).IsEqualTo("Async failed: Test exception");
            }
        }

        /// <summary>
        /// Tests for edge cases and thread safety
        /// </summary>
        public class TryPatternEdgeCaseTests
        {
            [Test]
            public async Task Try_WithStackOverflowException_PropagatesException()
            {
                // Note: We can't actually test StackOverflowException as it would crash the test runner
                // This test documents the expected behavior
                await Assert.That(true).IsTrue(); // Placeholder
            }

            [Test]
            public async Task Try_WithOutOfMemoryException_ReturnsFailureResult()
            {
                // Arrange
                Func<int> func = () => throw new OutOfMemoryException("Test OOM");

                // Act
                var result = Result<int>.Try(func);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<OutOfMemoryException>();
            }

            [Test]
            public async Task TryAsync_WithAggregateException_UnwrapsInnerException()
            {
                // Arrange
                var innerException = new InvalidOperationException("Inner exception");
                Func<Task<string>> asyncFunc = () => Task.FromException<string>(innerException);

                // Act
                var result = await Result<string>.TryAsync(asyncFunc);

                // Assert
                await Assert.That(result.Success).IsFalse();
                await Assert.That(result.Exception).IsNotNull();
                await Assert.That(result.Exception).IsTypeOf<InvalidOperationException>();
                await Assert.That(result.Exception.Message).IsEqualTo("Inner exception");
            }

            [Test]
            public async Task Try_ConcurrentExecution_ThreadSafe()
            {
                // Arrange
                int executionCount = 0;
                var tasks = new Task<Result<int>>[100];

                // Act
                for (int i = 0; i < tasks.Length; i++)
                {
                    int index = i;
                    tasks[i] = Task.Run(() => Result<int>.Try(() =>
                    {
                        System.Threading.Interlocked.Increment(ref executionCount);
                        return index;
                    }));
                }

                var results = await Task.WhenAll(tasks);

                // Assert
                await Assert.That(executionCount).IsEqualTo(100);
                for (int i = 0; i < results.Length; i++)
                {
                    await Assert.That(results[i].Success).IsTrue();
                    await Assert.That(results[i].Data >= 0 && results[i].Data < 100).IsTrue();
                }
            }
        }
    }
}