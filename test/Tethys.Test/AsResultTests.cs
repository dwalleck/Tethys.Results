using TUnit.Assertions;
using TUnit.Core;
using Tethys.Results;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for the AsResult method that converts Result&lt;T&gt; to Result.
    /// </summary>
    public class AsResultTests
    {
        [Test]
        public async Task AsResult_WithSuccessfulResult_ReturnsSuccessResult()
        {
            // Arrange
            var result = Result<int>.Ok(42, "Success message");
            
            // Act
            var nonGenericResult = result.AsResult();
            
            // Assert
            await Assert.That(nonGenericResult.Success).IsTrue();
            await Assert.That(nonGenericResult.Message).IsEqualTo("Success message");
            await Assert.That(nonGenericResult.Exception).IsNull();
        }
        
        [Test]
        public async Task AsResult_WithFailureResultAndMessage_ReturnsFailureResult()
        {
            // Arrange
            var result = Result<int>.Fail("Error occurred");
            
            // Act
            var nonGenericResult = result.AsResult();
            
            // Assert
            await Assert.That(nonGenericResult.Success).IsFalse();
            await Assert.That(nonGenericResult.Message).IsEqualTo("Error occurred");
            await Assert.That(nonGenericResult.Exception).IsNull();
        }
        
        [Test]
        public async Task AsResult_WithFailureResultNoException_CallsFailWithoutException()
        {
            // This tests that AsResult properly handles the case where the generic result
            // has no exception by calling the appropriate Fail overload
            
            // Arrange - Create a failed result without an exception
            var result = Result<int>.Fail("No exception error");
            
            // Act
            var nonGenericResult = result.AsResult();
            
            // Assert
            await Assert.That(nonGenericResult.Success).IsFalse();
            await Assert.That(nonGenericResult.Message).IsEqualTo("No exception error");
            await Assert.That(nonGenericResult.Exception).IsNull();
        }
        
        [Test]
        public async Task AsResult_WithFailureResultAndException_ReturnsFailureResultWithException()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            var result = Result<int>.Fail("Error with exception", exception);
            
            // Act
            var nonGenericResult = result.AsResult();
            
            // Assert
            await Assert.That(nonGenericResult.Success).IsFalse();
            await Assert.That(nonGenericResult.Message).IsEqualTo("Error with exception");
            await Assert.That(nonGenericResult.Exception).IsEqualTo(exception);
        }
        
        [Test]
        public async Task AsResult_WithDefaultSuccessMessage_PreservesMessage()
        {
            // Arrange
            var result = Result<string>.Ok("test data");
            
            // Act
            var nonGenericResult = result.AsResult();
            
            // Assert
            await Assert.That(nonGenericResult.Success).IsTrue();
            await Assert.That(nonGenericResult.Message).IsEqualTo("Operation completed successfully");
            await Assert.That(nonGenericResult.Exception).IsNull();
        }
    }
}