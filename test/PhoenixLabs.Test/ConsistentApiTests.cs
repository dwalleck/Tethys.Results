using System;
using System.Text;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace PhoenixLabs.Test
{
    public class ConsistentApiTests
    {
        [Test]
        public async Task Ok_WithNoParameters_ShouldReturnSuccessfulResult()
        {
            // Act
            var result = Result.Ok();
            
            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
            await Assert.That(result.Exception).IsNull();
        }
        
        [Test]
        public async Task Ok_WithDataOnly_ShouldReturnSuccessfulResultWithData()
        {
            // Arrange
            var testData = "Test data";
            
            // Act
            var result = Result<string>.Ok(testData);
            
            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
            await Assert.That(result.Data).IsEqualTo(testData);
            await Assert.That(result.Exception).IsNull();
        }
        
        [Test]
        public async Task Fail_WithMessageOnly_ShouldReturnFailedResult()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act
            var result = Result.Fail(errorMessage);
            
            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo(errorMessage);
            await Assert.That(result.Exception).IsNull();
        }
        
        [Test]
        public async Task GenericFail_WithMessageOnly_ShouldReturnFailedResultWithoutData()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act
            var result = Result<string>.Fail(errorMessage);
            
            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo(errorMessage);
            await Assert.That(result.Data).IsEqualTo(default(string));
            await Assert.That(result.Exception).IsNull();
        }
        
        [Test]
        public async Task Fail_WithMessageAndException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act & Assert
            try
            {
                var result = Result.Fail(errorMessage, null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }
        
        [Test]
        public async Task GenericFail_WithMessageAndException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act & Assert
            try
            {
                var result = Result<string>.Fail(errorMessage, null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }
        
        [Test]
        public async Task ConsistentAPI_ShouldAllowChaining_WithDifferentOverloads()
        {
            // Arrange
            var executionOrder = new StringBuilder();
            
            // Act
            var result = Result.Ok()
                .Then(() => {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .Then(() => {
                    executionOrder.Append("2-");
                    var data = "Test data";
                    return Result<string>.Ok(data)
                        .Then(value => {
                            executionOrder.Append("3-");
                            return Result.Ok();
                        });
                })
                .Then(() => {
                    executionOrder.Append("4");
                    return Result.Ok("Final operation");
                });
            
            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3-4");
        }
        
        [Test]
        public async Task ConsistentAPI_ShouldAllowChaining_WithErrorHandling()
        {
            // Arrange
            var executionOrder = new StringBuilder();
            
            // Act
            var result = Result.Ok()
                .Then(() => {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .Then(() => {
                    executionOrder.Append("2-");
                    return Result.Fail("Operation failed");
                })
                .Then(() => {
                    executionOrder.Append("3");
                    return Result.Ok("This should not execute");
                });
            
            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Operation failed");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-");
        }
        
        [Test]
        public async Task ConsistentAPI_ShouldWorkWithAsyncMethods()
        {
            // Arrange
            var executionOrder = new StringBuilder();
            
            // Act
            var result = await Result.Ok()
                .ThenAsync(async () => {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .ThenAsync(async () => {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    var data = "Test data";
                    return await Result<string>.Ok(data)
                        .ThenAsync(async value => {
                            await Task.Delay(10); // Simulate async work
                            executionOrder.Append("3-");
                            return Result.Ok();
                        });
                })
                .ThenAsync(async () => {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("4");
                    return Result.Ok("Final operation");
                });
            
            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3-4");
        }
        
        [Test]
        public async Task ConsistentAPI_ShouldProvideConsistentBehaviorAcrossOverloads()
        {
            // Test Result.Ok() and Result.Ok(message)
            var result1 = Result.Ok();
            var result2 = Result.Ok("Custom message");
            
            await Assert.That(result1.Success).IsTrue();
            await Assert.That(result1.Message).IsEqualTo("Operation completed successfully");
            await Assert.That(result2.Success).IsTrue();
            await Assert.That(result2.Message).IsEqualTo("Custom message");
            
            // Test Result<T>.Ok(data) and Result<T>.Ok(data, message)
            var result3 = Result<int>.Ok(42);
            var result4 = Result<int>.Ok(42, "Custom message");
            
            await Assert.That(result3.Success).IsTrue();
            await Assert.That(result3.Data).IsEqualTo(42);
            await Assert.That(result3.Message).IsEqualTo("Operation completed successfully");
            await Assert.That(result4.Success).IsTrue();
            await Assert.That(result4.Data).IsEqualTo(42);
            await Assert.That(result4.Message).IsEqualTo("Custom message");
            
            // Test Result.Fail(message) and Result.Fail(message, exception)
            var result5 = Result.Fail("Error message");
            var exception = new Exception("Test exception");
            var result6 = Result.Fail("Error message", exception);
            
            await Assert.That(result5.Success).IsFalse();
            await Assert.That(result5.Message).IsEqualTo("Error message");
            await Assert.That(result5.Exception).IsNull();
            await Assert.That(result6.Success).IsFalse();
            await Assert.That(result6.Message).IsEqualTo("Error message");
            await Assert.That(result6.Exception).IsEqualTo(exception);
            
            // Test Result<T>.Fail(message) and Result<T>.Fail(message, exception)
            var result7 = Result<int>.Fail("Error message");
            var result8 = Result<int>.Fail("Error message", exception);
            
            await Assert.That(result7.Success).IsFalse();
            await Assert.That(result7.Message).IsEqualTo("Error message");
            await Assert.That(result7.Data).IsEqualTo(default(int));
            await Assert.That(result7.Exception).IsNull();
            await Assert.That(result8.Success).IsFalse();
            await Assert.That(result8.Message).IsEqualTo("Error message");
            await Assert.That(result8.Data).IsEqualTo(default(int));
            await Assert.That(result8.Exception).IsEqualTo(exception);
        }
    }
}
