using System.Text;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class AsyncTests
    {
        [Test]
        public async Task ThenAsync_ShouldExecuteNextAsyncOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result.Ok("First operation");
            bool nextOperationExecuted = false;

            // Act
            var finalResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task ThenAsync_ShouldNotExecuteNextAsyncOperation_WhenPreviousFailed()
        {
            // Arrange
            var result = Result.Fail("First operation failed");
            bool nextOperationExecuted = false;

            // Act
            var finalResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(nextOperationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("First operation failed");
        }

        [Test]
        public async Task ThenAsync_WithData_ShouldPassDataToNextAsyncOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");
            int passedValue = 0;

            // Act
            var finalResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task ThenAsync_WithData_ShouldTransformDataAsynchronously_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");

            // Act
            var finalResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                return Result<string>.Ok(value.ToString(), "Transformed data");
            });

            // Assert
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Data).IsEqualTo("42");
            await Assert.That(finalResult.Message).IsEqualTo("Transformed data");
        }

        [Test]
        public async Task ThenAsync_WithTaskResult_ShouldChainAsyncOperations()
        {
            // Arrange
            Task<Result> asyncResult = Task.FromResult(Result.Ok("First operation"));
            bool nextOperationExecuted = false;

            // Act
            var finalResult = await asyncResult.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task ThenAsync_WithTaskGenericResult_ShouldChainAsyncOperationsWithData()
        {
            // Arrange
            Task<Result<int>> asyncResult = Task.FromResult(Result<int>.Ok(42, "First operation"));
            int passedValue = 0;

            // Act
            var finalResult = await asyncResult.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task ComplexAsyncPipeline_ShouldExecuteAllOperationsAsynchronously()
        {
            // Arrange
            var executionOrder = new StringBuilder();

            // Act
            var result = await Result.Ok("Start")
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Ok("Second operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("Final operation");
                });

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }

        [Test]
        public async Task ComplexAsyncPipeline_ShouldStopAtFirstFailure()
        {
            // Arrange
            var executionOrder = new StringBuilder();

            // Act
            var result = await Result.Ok("Start")
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Fail("Operation failed");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("This should not execute");
                });

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Operation failed");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-");
        }

        [Test]
        public async Task MixedSyncAndAsyncPipeline_ShouldWorkCorrectly()
        {
            // Arrange
            var executionOrder = new StringBuilder();

            // Act
            var result = await Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("Sync operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Ok("Async operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("Final sync operation");
                });

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final sync operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }
    }
}
