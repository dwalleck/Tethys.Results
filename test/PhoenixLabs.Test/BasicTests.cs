using System;
using System.Text;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class BasicTests
    {
        [Test]
        public async Task Result_Ok_ShouldReturnSuccessfulResult()
        {
            // Act
            var result = Result.Ok("Success message");

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Success message");
            await Assert.That(result.Exception).IsNull();
        }

        [Test]
        public async Task Result_Fail_ShouldReturnFailedResult()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            var result = Result.Fail("Error message", exception);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Error message");
            await Assert.That(result.Exception).IsEqualTo(exception);
        }

        [Test]
        public async Task GenericResult_Ok_ShouldReturnSuccessfulResultWithData()
        {
            // Arrange
            var testData = "Test data";

            // Act
            var result = Result<string>.Ok(testData, "Success message");

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Success message");
            await Assert.That(result.Data).IsEqualTo(testData);
            await Assert.That(result.Exception).IsNull();
        }

        [Test]
        public async Task GenericResult_Fail_ShouldReturnFailedResultWithoutData()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            var result = Result<string>.Fail("Error message", exception);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Error message");
            await Assert.That(result.Data).IsEqualTo(default(string));
            await Assert.That(result.Exception).IsEqualTo(exception);
        }

        [Test]
        public async Task Then_ShouldExecuteNextOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result.Ok("First operation");
            bool nextOperationExecuted = false;

            // Act
            var finalResult = result.Then(() =>
            {
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task Then_ShouldNotExecuteNextOperation_WhenPreviousFailed()
        {
            // Arrange
            var result = Result.Fail("First operation failed");
            bool nextOperationExecuted = false;

            // Act
            var finalResult = result.Then(() =>
            {
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(nextOperationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("First operation failed");
        }

        [Test]
        public async Task Then_WithData_ShouldPassDataToNextOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");
            int passedValue = 0;

            // Act
            var finalResult = result.Then(value =>
            {
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert
            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }

        [Test]
        public async Task Then_WithData_ShouldTransformData_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");

            // Act
            var finalResult = result.Then(value =>
                Result<string>.Ok(value.ToString(), "Transformed data")
            );

            // Assert
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Data).IsEqualTo("42");
            await Assert.That(finalResult.Message).IsEqualTo("Transformed data");
        }

        [Test]
        public async Task When_ShouldExecuteOperation_WhenConditionIsTrue()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            bool operationExecuted = false;

            // Act
            var finalResult = result.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert
            await Assert.That(operationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Conditional operation");
        }

        [Test]
        public async Task When_ShouldNotExecuteOperation_WhenConditionIsFalse()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            bool operationExecuted = false;

            // Act
            var finalResult = result.When(false, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert
            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Initial result");
        }

        [Test]
        public async Task When_ShouldNotExecuteOperation_WhenPreviousResultFailed()
        {
            // Arrange
            var result = Result.Fail("Initial failure");
            bool operationExecuted = false;

            // Act
            var finalResult = result.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert
            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("Initial failure");
        }

        [Test]
        public async Task ComplexPipeline_ShouldExecuteAllOperations_AndReturnFinalResult()
        {
            // Arrange
            var executionOrder = new StringBuilder();

            // Act
            var result = Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .When(true, () =>
                {
                    executionOrder.Append("2-");
                    return Result.Ok("Conditional operation");
                })
                .Then(() =>
                {
                    executionOrder.Append("3");
                    return Result.Ok("Final operation");
                });

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }

        [Test]
        public async Task ComplexPipeline_ShouldStopAtFirstFailure()
        {
            // Arrange
            var executionOrder = new StringBuilder();

            // Act
            var result = Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .Then(() =>
                {
                    executionOrder.Append("2-");
                    return Result.Fail("Operation failed");
                })
                .Then(() =>
                {
                    executionOrder.Append("3");
                    return Result.Ok("This should not execute");
                });

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Operation failed");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-");
        }
    }
}
