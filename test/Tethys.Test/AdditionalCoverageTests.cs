using TUnit.Assertions;
using TUnit.Core;
using Tethys.Results;

namespace Tethys.Test
{
    /// <summary>
    /// Additional tests to improve code coverage for methods with low coverage.
    /// </summary>
    public class AdditionalCoverageTests
    {
        #region Fail Method Edge Cases
        
        [Test]
        public async Task Result_Fail_WithNullMessage_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result.Fail(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("message");
        }
        
        [Test]
        public async Task GenericResult_Fail_WithNullMessage_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result<int>.Fail(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("message");
        }
        
        #endregion
        
        #region FromException Edge Cases
        
        [Test]
        public async Task Result_FromException_WithNullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result.FromException(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("exception");
        }
        
        [Test]
        public async Task GenericResult_FromException_WithNullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result<string>.FromException(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("exception");
        }
        
        #endregion
        
        #region Equals Method Additional Cases
        
        [Test]
        public async Task Result_Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var result = Result.Ok();
            
            // Act
            var isEqual = result.Equals(null);
            
            // Assert
            await Assert.That(isEqual).IsFalse();
        }
        
        [Test]
        public async Task Result_Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var result = Result.Ok();
            
            // Act
            var isEqual = result.Equals("not a result");
            
            // Assert
            await Assert.That(isEqual).IsFalse();
        }
        
        [Test]
        public async Task GenericResult_Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            
            // Act
            var isEqual = result.Equals(null);
            
            // Assert
            await Assert.That(isEqual).IsFalse();
        }
        
        [Test]
        public async Task GenericResult_Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            
            // Act
            var isEqual = result.Equals("not a result");
            
            // Assert
            await Assert.That(isEqual).IsFalse();
        }
        
        [Test]
        public async Task GenericResult_Equals_WithDifferentGenericType_ReturnsFalse()
        {
            // Arrange
            var intResult = Result<int>.Ok(42);
            var stringResult = Result<string>.Ok("42");
            
            // Act
            var isEqual = intResult.Equals(stringResult);
            
            // Assert
            await Assert.That(isEqual).IsFalse();
        }
        
        #endregion
        
        #region Try Method Edge Cases
        
        [Test]
        public async Task Result_Try_WithNullAction_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result.Try(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("action");
        }
        
        [Test]
        public async Task GenericResult_Try_WithNullFunc_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result<int>.Try(null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("func");
        }
        
        #endregion
        
        #region Combine Method Edge Cases
        
        [Test]
        public async Task Result_Combine_WithEmptyEnumerable_ThrowsArgumentException()
        {
            // Arrange
            var results = Enumerable.Empty<Result>();
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                Result.Combine(results);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("results");
        }
        
        [Test]
        public async Task GenericResult_Combine_WithEmptyEnumerable_ThrowsArgumentException()
        {
            // Arrange
            var results = Enumerable.Empty<Result<int>>();
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                Result<int>.Combine(results);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("results");
        }
        
        #endregion
        
        #region Additional Edge Cases
        
        [Test]
        public async Task Result_Fail_WithNullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result.Fail("Error message", null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("exception");
        }
        
        [Test]
        public async Task GenericResult_Fail_WithNullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result<int>.Fail("Error message", null);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("exception");
        }
        
        #endregion
    }
}