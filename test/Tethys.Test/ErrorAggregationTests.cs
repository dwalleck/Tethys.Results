using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class ErrorAggregationTests
    {
        [Test]
        public async Task Combine_ShouldReturnSuccessfulResult_WhenAllResultsAreSuccessful()
        {
            // Arrange
            var results = new List<Result>
            {
                Result.Ok("First operation"),
                Result.Ok("Second operation"),
                Result.Ok("Third operation")
            };
            
            // Act
            var combinedResult = Result.Combine(results);
            
            // Assert
            await Assert.That(combinedResult.Success).IsTrue();
            await Assert.That(combinedResult.Message).IsEqualTo("All operations completed successfully");
        }
        
        [Test]
        public async Task Combine_ShouldReturnFailedResult_WhenAnyResultIsFailed()
        {
            // Arrange
            var results = new List<Result>
            {
                Result.Ok("First operation"),
                Result.Fail("Second operation failed"),
                Result.Ok("Third operation")
            };
            
            // Act
            var combinedResult = Result.Combine(results);
            
            // Assert
            await Assert.That(combinedResult.Success).IsFalse();
            await Assert.That(combinedResult.Message).IsEqualTo("One or more operations failed");
            await Assert.That(combinedResult.Exception).IsNotNull();
            await Assert.That(combinedResult.Exception is AggregateError).IsTrue();
        }
        
        [Test]
        public async Task Combine_ShouldAggregateErrorMessages_WhenMultipleResultsFail()
        {
            // Arrange
            var results = new List<Result>
            {
                Result.Ok("First operation"),
                Result.Fail("Second operation failed"),
                Result.Fail("Third operation failed")
            };
            
            // Act
            var combinedResult = Result.Combine(results);
            var aggregateError = combinedResult.Exception as AggregateError;
            
            // Assert
            await Assert.That(combinedResult.Success).IsFalse();
            await Assert.That(aggregateError).IsNotNull();
            await Assert.That(aggregateError.ErrorMessages.Count).IsEqualTo(2);
            await Assert.That(aggregateError.ErrorMessages[0]).IsEqualTo("Second operation failed");
            await Assert.That(aggregateError.ErrorMessages[1]).IsEqualTo("Third operation failed");
        }
        
        [Test]
        public async Task Combine_ShouldAggregateExceptions_WhenMultipleResultsFailWithExceptions()
        {
            // Arrange
            var exception1 = new ArgumentException("Invalid argument");
            var exception2 = new InvalidOperationException("Invalid operation");
            
            var results = new List<Result>
            {
                Result.Ok("First operation"),
                Result.Fail("Second operation failed", exception1),
                Result.Fail("Third operation failed", exception2)
            };
            
            // Act
            var combinedResult = Result.Combine(results);
            var aggregateError = combinedResult.Exception as AggregateError;
            
            // Assert
            await Assert.That(combinedResult.Success).IsFalse();
            await Assert.That(aggregateError).IsNotNull();
            await Assert.That(aggregateError.InnerErrors.Count).IsEqualTo(2);
            await Assert.That(aggregateError.InnerErrors[0]).IsEqualTo(exception1);
            await Assert.That(aggregateError.InnerErrors[1]).IsEqualTo(exception2);
        }
        
        [Test]
        public async Task GenericCombine_ShouldReturnSuccessfulResult_WhenAllResultsAreSuccessful()
        {
            // Arrange
            var results = new List<Result<int>>
            {
                Result<int>.Ok(1),
                Result<int>.Ok(2),
                Result<int>.Ok(3)
            };
            
            // Act
            var combinedResult = Result<int>.Combine(results);
            
            // Assert
            await Assert.That(combinedResult.Success).IsTrue();
            await Assert.That(combinedResult.Message).IsEqualTo("All operations completed successfully");
            await Assert.That(combinedResult.Data).IsNotNull();
            
            var data = combinedResult.Data.ToList();
            await Assert.That(data.Count).IsEqualTo(3);
            await Assert.That(data[0]).IsEqualTo(1);
            await Assert.That(data[1]).IsEqualTo(2);
            await Assert.That(data[2]).IsEqualTo(3);
        }
        
        [Test]
        public async Task GenericCombine_ShouldReturnFailedResult_WhenAnyResultIsFailed()
        {
            // Arrange
            var results = new List<Result<int>>
            {
                Result<int>.Ok(1),
                Result<int>.Fail("Second operation failed"),
                Result<int>.Ok(3)
            };
            
            // Act
            var combinedResult = Result<int>.Combine(results);
            
            // Assert
            await Assert.That(combinedResult.Success).IsFalse();
            await Assert.That(combinedResult.Message).IsEqualTo("One or more operations failed");
            await Assert.That(combinedResult.Exception).IsNotNull();
            await Assert.That(combinedResult.Exception is AggregateError).IsTrue();
            await Assert.That(combinedResult.Data).IsNull();
        }
        
        [Test]
        public async Task Combine_ShouldThrowArgumentNullException_WhenResultsIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result.Combine(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("results");
            }
        }
        
        [Test]
        public async Task Combine_ShouldThrowArgumentException_WhenResultsIsEmpty()
        {
            // Act & Assert
            try
            {
                var result = Result.Combine(new List<Result>());
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("results");
            }
        }
        
        [Test]
        public async Task GenericCombine_ShouldThrowArgumentNullException_WhenResultsIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result<int>.Combine(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("results");
            }
        }
        
        [Test]
        public async Task GenericCombine_ShouldThrowArgumentException_WhenResultsIsEmpty()
        {
            // Act & Assert
            try
            {
                var result = Result<int>.Combine(new List<Result<int>>());
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("results");
            }
        }
        
        [Test]
        public async Task AggregateError_ShouldContainAllErrorMessages()
        {
            // Arrange
            var errorMessages = new List<string> { "Error 1", "Error 2", "Error 3" };
            
            // Act
            var aggregateError = new AggregateError(errorMessages);
            
            // Assert
            await Assert.That(aggregateError.ErrorMessages.Count).IsEqualTo(3);
            await Assert.That(aggregateError.ErrorMessages[0]).IsEqualTo("Error 1");
            await Assert.That(aggregateError.ErrorMessages[1]).IsEqualTo("Error 2");
            await Assert.That(aggregateError.ErrorMessages[2]).IsEqualTo("Error 3");
            await Assert.That(aggregateError.Message).Contains("Error 1");
            await Assert.That(aggregateError.Message).Contains("Error 2");
            await Assert.That(aggregateError.Message).Contains("Error 3");
        }
        
        [Test]
        public async Task AggregateError_ShouldContainAllInnerExceptions()
        {
            // Arrange
            var exceptions = new List<Exception> 
            { 
                new ArgumentException("Invalid argument"),
                new InvalidOperationException("Invalid operation"),
                new NullReferenceException("Null reference")
            };
            
            // Act
            var aggregateError = new AggregateError(exceptions);
            
            // Assert
            await Assert.That(aggregateError.InnerErrors.Count).IsEqualTo(3);
            await Assert.That(aggregateError.InnerErrors[0]).IsEqualTo(exceptions[0]);
            await Assert.That(aggregateError.InnerErrors[1]).IsEqualTo(exceptions[1]);
            await Assert.That(aggregateError.InnerErrors[2]).IsEqualTo(exceptions[2]);
            await Assert.That(aggregateError.ErrorMessages.Count).IsEqualTo(3);
            await Assert.That(aggregateError.ErrorMessages[0]).IsEqualTo("Invalid argument");
            await Assert.That(aggregateError.ErrorMessages[1]).IsEqualTo("Invalid operation");
            await Assert.That(aggregateError.ErrorMessages[2]).IsEqualTo("Null reference");
        }
    }
}
