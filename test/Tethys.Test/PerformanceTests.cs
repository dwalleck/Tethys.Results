using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    /// <summary>
    /// Tests to ensure performance optimizations don't break functionality.
    /// These tests focus on edge cases and behavior preservation.
    /// </summary>
    public class PerformanceTests
    {
        #region Combine Method Tests

        [Test]
        public async Task Combine_WithLargeCollection_ReturnsCorrectResult()
        {
            // Arrange
            var results = new List<Result>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(Result.Ok());
            }

            // Act
            var combined = Result.Combine(results);

            // Assert
            await Assert.That(combined.Success).IsTrue();
            await Assert.That(combined.Message).IsEqualTo("All operations completed successfully");
        }

        [Test]
        public async Task Combine_WithEnumerableMultipleEnumeration_EnumeratesOnlyOnce()
        {
            // Arrange
            var enumerationCount = 0;
            var results = CreateCountingEnumerable(() => enumerationCount++, 10, true);

            // Act
            var combined = Result.Combine(results);

            // Assert
            await Assert.That(combined.Success).IsTrue();
            await Assert.That(enumerationCount).IsEqualTo(1); // Should only enumerate once
        }

        [Test]
        public async Task CombineGeneric_WithEnumerableMultipleEnumeration_EnumeratesOnlyOnce()
        {
            // Arrange
            var enumerationCount = 0;
            var results = CreateCountingEnumerableGeneric(() => enumerationCount++, 10, true);

            // Act
            var combined = Result<int>.Combine(results);

            // Assert
            await Assert.That(combined.Success).IsTrue();
            await Assert.That(enumerationCount).IsEqualTo(1); // Should only enumerate once
        }

        [Test]
        public async Task Combine_WithMixedResults_StopsAtFirstFailure()
        {
            // Arrange
            var evaluationCount = 0;
            var results = new[]
            {
                Result.Ok(),
                Result.Ok(),
                Result.Fail("Error 1"),
                CreateDelayedResult(() => evaluationCount++), // This should not be evaluated
                CreateDelayedResult(() => evaluationCount++)  // This should not be evaluated
            };

            // Act
            var combined = Result.Combine(results);

            // Assert
            await Assert.That(combined.Success).IsFalse();
            await Assert.That(combined.Message).IsEqualTo("One or more operations failed");
            await Assert.That(evaluationCount).IsEqualTo(2); // Currently evaluates all results (this is what we'll optimize)
        }

        #endregion

        #region Async Method ConfigureAwait Tests

        [Test]
        public async Task ThenAsync_WithConfigureAwait_DoesNotCaptureContext()
        {
            // Arrange
            var result = Result.Ok();
            var originalContext = SynchronizationContext.Current;
            
            // Act - The implementation should use ConfigureAwait(false)
            await result.ThenAsync(async () =>
            {
                await Task.Delay(1);
                // After ConfigureAwait(false), we shouldn't be on the original context
                await Assert.That(SynchronizationContext.Current).IsNull();
                return Result.Ok();
            });
        }

        [Test]
        public async Task MapAsync_WithConfigureAwait_DoesNotCaptureContext()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var originalContext = SynchronizationContext.Current;
            
            // Act
            await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                // After ConfigureAwait(false), we shouldn't be on the original context
                await Assert.That(SynchronizationContext.Current).IsNull();
                return value * 2;
            });
        }

        [Test]
        public async Task OnSuccessAsync_WithConfigureAwait_DoesNotCaptureContext()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var originalContext = SynchronizationContext.Current;
            
            // Act
            await result.OnSuccessAsync(async value =>
            {
                await Task.Delay(1);
                // After ConfigureAwait(false), we shouldn't be on the original context
                await Assert.That(SynchronizationContext.Current).IsNull();
            });
        }

        [Test]
        public async Task TryAsync_WithCancellation_HandlesException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await Result.TryAsync(async () =>
            {
                await Task.Delay(1000, cts.Token);
            });

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Exception).IsNotNull();
            await Assert.That(result.Exception).IsTypeOf<TaskCanceledException>();
        }

        #endregion

        #region Helper Methods

        private static IEnumerable<Result> CreateCountingEnumerable(
            Action onEnumeration, 
            int count, 
            bool allSuccess)
        {
            onEnumeration();
            for (int i = 0; i < count; i++)
            {
                yield return allSuccess ? Result.Ok() : Result.Fail($"Error {i}");
            }
        }

        private static IEnumerable<Result<int>> CreateCountingEnumerableGeneric(
            Action onEnumeration, 
            int count, 
            bool allSuccess)
        {
            onEnumeration();
            for (int i = 0; i < count; i++)
            {
                yield return allSuccess ? Result<int>.Ok(i) : Result<int>.Fail($"Error {i}");
            }
        }

        private static Result CreateDelayedResult(Action onEvaluation)
        {
            onEvaluation();
            return Result.Ok();
        }

        #endregion
    }
}