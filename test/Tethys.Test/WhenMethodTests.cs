using TUnit.Assertions;
using TUnit.Core;
using Tethys.Results;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for the When extension method.
    /// </summary>
    public class WhenMethodTests
    {
        [Test]
        public async Task When_WithSuccessResultAndTrueCondition_ExecutesOperation()
        {
            // Arrange
            var initialResult = Result.Ok("Initial success");
            var operationExecuted = false;
            
            // Act
            var finalResult = initialResult.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Operation executed");
            });
            
            // Assert
            await Assert.That(operationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Operation executed");
        }
        
        [Test]
        public async Task When_WithSuccessResultAndFalseCondition_ReturnsOriginalResult()
        {
            // Arrange
            var initialResult = Result.Ok("Initial success");
            var operationExecuted = false;
            
            // Act
            var finalResult = initialResult.When(false, () =>
            {
                operationExecuted = true;
                return Result.Ok("Should not execute");
            });
            
            // Assert
            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Initial success");
        }
        
        [Test]
        public async Task When_WithFailureResult_ReturnsOriginalFailureRegardlessOfCondition()
        {
            // Arrange
            var initialResult = Result.Fail("Initial failure");
            var operationExecuted = false;
            
            // Act
            var finalResult = initialResult.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Should not execute");
            });
            
            // Assert
            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("Initial failure");
        }
        
        [Test]
        public async Task When_WithSuccessResultAndOperationReturnsFailure_ReturnsFailureResult()
        {
            // Arrange
            var initialResult = Result.Ok("Initial success");
            
            // Act
            var finalResult = initialResult.When(true, () => Result.Fail("Operation failed"));
            
            // Assert
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("Operation failed");
        }
        
        [Test]
        public async Task When_WithNullOperation_ThrowsNullReferenceException()
        {
            // Arrange
            var result = Result.Ok();
            Func<Result> nullOperation = null;
            
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
            {
                result.When(true, nullOperation);
                return Task.CompletedTask;
            });
        }
        
        [Test]
        public async Task When_CanBeChained_ExecutesInOrder()
        {
            // Arrange
            var callOrder = new List<int>();
            
            // Act
            var finalResult = Result.Ok("Start")
                .When(true, () =>
                {
                    callOrder.Add(1);
                    return Result.Ok("First");
                })
                .When(true, () =>
                {
                    callOrder.Add(2);
                    return Result.Ok("Second");
                })
                .When(false, () =>
                {
                    callOrder.Add(3); // Should not execute
                    return Result.Ok("Third");
                });
            
            // Assert
            await Assert.That(callOrder).IsEquivalentTo(new[] { 1, 2 });
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second");
        }
        
        [Test]
        public async Task When_ChainStopsOnFailure_DoesNotExecuteSubsequentOperations()
        {
            // Arrange
            var callOrder = new List<int>();
            
            // Act
            var finalResult = Result.Ok("Start")
                .When(true, () =>
                {
                    callOrder.Add(1);
                    return Result.Fail("Failed at first");
                })
                .When(true, () =>
                {
                    callOrder.Add(2); // Should not execute
                    return Result.Ok("Second");
                });
            
            // Assert
            await Assert.That(callOrder).IsEquivalentTo(new[] { 1 });
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("Failed at first");
        }
    }
}