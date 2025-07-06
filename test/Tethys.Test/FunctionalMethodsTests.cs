using TUnit;
using Tethys.Results;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for functional programming methods (Match, Map, FlatMap, MapError).
    /// Written following TDD principles.
    /// </summary>
    public class FunctionalMethodsTests
    {
        #region Match Tests for Result

        [Test]
        public async Task Match_OnSuccessResult_ShouldCallOnSuccessFunction()
        {
            // Arrange
            var result = Result.Ok("Operation successful");
            var onSuccessCalled = false;
            var onFailureCalled = false;

            // Act
            var output = result.Match(
                onSuccess: () => 
                {
                    onSuccessCalled = true;
                    return "Success branch";
                },
                onFailure: (ex) => 
                {
                    onFailureCalled = true;
                    return "Failure branch";
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsTrue();
            await Assert.That(onFailureCalled).IsFalse();
            await Assert.That(output).IsEqualTo("Success branch");
        }

        [Test]
        public async Task Match_OnFailureResult_ShouldCallOnFailureFunction()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result.Fail("Operation failed", exception);
            var onSuccessCalled = false;
            var onFailureCalled = false;
            Exception capturedError = null;

            // Act
            var output = result.Match(
                onSuccess: () => 
                {
                    onSuccessCalled = true;
                    return "Success branch";
                },
                onFailure: (ex) => 
                {
                    onFailureCalled = true;
                    capturedError = ex;
                    return "Failure branch";
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsFalse();
            await Assert.That(onFailureCalled).IsTrue();
            await Assert.That(output).IsEqualTo("Failure branch");
            await Assert.That(capturedError).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task Match_OnFailureResultWithoutException_ShouldPassNullToOnFailure()
        {
            // Arrange
            var result = Result.Fail("Simple failure");
            Exception capturedError = null;
            var errorHandled = false;

            // Act
            var output = result.Match(
                onSuccess: () => "Success",
                onFailure: (ex) => 
                {
                    errorHandled = true;
                    capturedError = ex;
                    return "Handled null exception";
                }
            );

            // Assert
            await Assert.That(errorHandled).IsTrue();
            await Assert.That(capturedError).IsNull();
            await Assert.That(output).IsEqualTo("Handled null exception");
        }

        #endregion

        #region Match Tests for Result<T>

        [Test]
        public async Task MatchGeneric_OnSuccessResult_ShouldCallOnSuccessWithValue()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var onSuccessCalled = false;
            var onFailureCalled = false;
            var capturedValue = 0;

            // Act
            var output = result.Match(
                onSuccess: (value) => 
                {
                    onSuccessCalled = true;
                    capturedValue = value;
                    return $"Value is {value}";
                },
                onFailure: (ex) => 
                {
                    onFailureCalled = true;
                    return "Failure";
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsTrue();
            await Assert.That(onFailureCalled).IsFalse();
            await Assert.That(capturedValue).IsEqualTo(42);
            await Assert.That(output).IsEqualTo("Value is 42");
        }

        [Test]
        public async Task MatchGeneric_OnFailureResult_ShouldCallOnFailureFunction()
        {
            // Arrange
            var exception = new ArgumentException("Invalid argument");
            var result = Result<string>.Fail("Operation failed", exception);
            var onSuccessCalled = false;
            var onFailureCalled = false;

            // Act
            var output = result.Match(
                onSuccess: (value) => 
                {
                    onSuccessCalled = true;
                    return value.ToUpper();
                },
                onFailure: (ex) => 
                {
                    onFailureCalled = true;
                    return ex?.Message ?? "No exception";
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsFalse();
            await Assert.That(onFailureCalled).IsTrue();
            await Assert.That(output).IsEqualTo("Invalid argument");
        }

        #endregion

        #region Match Null Parameter Tests

        [Test]
        public async Task Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result.Ok();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Task.FromResult(result.Match<string>(
                    onSuccess: null,
                    onFailure: (ex) => "failure"
                ))
            );
        }

        [Test]
        public async Task Match_WithNullOnFailure_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result.Ok();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Task.FromResult(result.Match<string>(
                    onSuccess: () => "success",
                    onFailure: null
                ))
            );
        }

        [Test]
        public async Task MatchGeneric_WithNullOnSuccess_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Task.FromResult(result.Match<string>(
                    onSuccess: null,
                    onFailure: (ex) => "failure"
                ))
            );
        }

        [Test]
        public async Task MatchGeneric_WithNullOnFailure_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Task.FromResult(result.Match<string>(
                    onSuccess: (value) => "success",
                    onFailure: null
                ))
            );
        }

        #endregion

        #region Async Match Tests

        [Test]
        public async Task MatchAsync_OnSuccessResult_ShouldAwaitOnSuccessFunction()
        {
            // Arrange
            var result = Result.Ok("Success");
            var onSuccessCalled = false;

            // Act
            var output = await result.MatchAsync(
                onSuccess: async () => 
                {
                    await Task.Delay(10);
                    onSuccessCalled = true;
                    return 100;
                },
                onFailure: async (ex) => 
                {
                    await Task.Delay(10);
                    return -1;
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsTrue();
            await Assert.That(output).IsEqualTo(100);
        }

        [Test]
        public async Task MatchAsync_OnFailureResult_ShouldAwaitOnFailureFunction()
        {
            // Arrange
            var result = Result.Fail("Failed");
            var onFailureCalled = false;

            // Act
            var output = await result.MatchAsync(
                onSuccess: async () => 
                {
                    await Task.Delay(10);
                    return 100;
                },
                onFailure: async (ex) => 
                {
                    await Task.Delay(10);
                    onFailureCalled = true;
                    return -1;
                }
            );

            // Assert
            await Assert.That(onFailureCalled).IsTrue();
            await Assert.That(output).IsEqualTo(-1);
        }

        [Test]
        public async Task MatchAsyncGeneric_OnSuccessResult_ShouldAwaitOnSuccessFunction()
        {
            // Arrange
            var result = Result<int>.Ok(10);
            var onSuccessCalled = false;

            // Act
            var output = await result.MatchAsync(
                onSuccess: async (value) => 
                {
                    await Task.Delay(10);
                    onSuccessCalled = true;
                    return value * 2;
                },
                onFailure: async (ex) => 
                {
                    await Task.Delay(10);
                    return -1;
                }
            );

            // Assert
            await Assert.That(onSuccessCalled).IsTrue();
            await Assert.That(output).IsEqualTo(20);
        }

        [Test]
        public async Task MatchAsyncGeneric_OnFailureResult_ShouldAwaitOnFailureFunction()
        {
            // Arrange
            var result = Result<int>.Fail("Async operation failed");
            var onFailureCalled = false;

            // Act
            var output = await result.MatchAsync(
                onSuccess: async (value) => 
                {
                    await Task.Delay(10);
                    return value * 2;
                },
                onFailure: async (ex) => 
                {
                    await Task.Delay(10);
                    onFailureCalled = true;
                    return -1;
                }
            );

            // Assert
            await Assert.That(onFailureCalled).IsTrue();
            await Assert.That(output).IsEqualTo(-1);
        }

        #endregion

        #region Map Tests

        [Test]
        public async Task Map_OnSuccessResult_ShouldTransformValue()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var mapped = result.Map(x => x * 2);

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Data).IsEqualTo(10);
            await Assert.That(mapped.Message).IsEqualTo("Operation completed successfully");
        }

        [Test]
        public async Task Map_OnFailureResult_ShouldReturnFailure()
        {
            // Arrange
            var exception = new InvalidOperationException("Failed");
            var result = Result<int>.Fail("Operation failed", exception);

            // Act
            var mapped = result.Map(x => x * 2);

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Data).IsEqualTo(0);
            await Assert.That(mapped.Message).IsEqualTo("Operation failed");
            await Assert.That(mapped.Exception).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task Map_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                Task.FromResult(result.Map<string>(null)));
        }

        [Test]
        public async Task Map_TransformToComplexType_ShouldWorkCorrectly()
        {
            // Arrange
            var result = Result<string>.Ok("John");

            // Act
            var mapped = result.Map(name => new { Name = name, Length = name.Length });

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Data.Name).IsEqualTo("John");
            await Assert.That(mapped.Data.Length).IsEqualTo(4);
        }

        [Test]
        public async Task Map_WhenMapperThrows_ShouldReturnFailureResult()
        {
            // Arrange
            var result = Result<int>.Ok(5);
            var mapperException = new InvalidOperationException("Mapper failed");

            // Act
            var mapped = result.Map<string>(x => throw mapperException);

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsSameReferenceAs(mapperException);
            await Assert.That(mapped.Message).Contains("Mapper failed");
        }

        #endregion

        #region MapAsync Tests

        [Test]
        public async Task MapAsync_OnSuccessResult_ShouldTransformValue()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var mapped = await result.MapAsync(async x => 
            {
                await Task.Delay(10);
                return x * 2;
            });

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Data).IsEqualTo(10);
        }

        [Test]
        public async Task MapAsync_OnFailureResult_ShouldReturnFailure()
        {
            // Arrange
            var result = Result<int>.Fail("Failed");

            // Act
            var mapped = await result.MapAsync(async x => 
            {
                await Task.Delay(10);
                return x * 2;
            });

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Message).IsEqualTo("Failed");
        }

        [Test]
        public async Task MapAsync_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await result.MapAsync<string>(null)
            );
        }

        #endregion

        #region FlatMap Tests

        [Test]
        public async Task FlatMap_OnSuccessResult_ShouldChainOperations()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var flatMapped = result.FlatMap(x => Result<string>.Ok($"Value: {x}"));

            // Assert
            await Assert.That(flatMapped.Success).IsTrue();
            await Assert.That(flatMapped.Data).IsEqualTo("Value: 5");
        }

        [Test]
        public async Task FlatMap_OnFailureResult_ShouldPropagateFailure()
        {
            // Arrange
            var exception = new InvalidOperationException("Initial failure");
            var result = Result<int>.Fail("Failed", exception);

            // Act
            var flatMapped = result.FlatMap(x => Result<string>.Ok($"Value: {x}"));

            // Assert
            await Assert.That(flatMapped.Success).IsFalse();
            await Assert.That(flatMapped.Message).IsEqualTo("Failed");
            await Assert.That(flatMapped.Exception).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task FlatMap_WhenMappedOperationFails_ShouldReturnFailure()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var flatMapped = result.FlatMap(x => Result<string>.Fail("Mapped operation failed"));

            // Assert
            await Assert.That(flatMapped.Success).IsFalse();
            await Assert.That(flatMapped.Message).IsEqualTo("Mapped operation failed");
        }

        [Test]
        public async Task FlatMap_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                Task.FromResult(result.FlatMap<string>(null)));
        }

        [Test]
        public async Task FlatMap_ChainMultipleOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var final = result
                .FlatMap(x => Result<int>.Ok(x * 2))
                .FlatMap(x => Result<string>.Ok($"Final: {x}"));

            // Assert
            await Assert.That(final.Success).IsTrue();
            await Assert.That(final.Data).IsEqualTo("Final: 10");
        }

        [Test]
        public async Task FlatMap_WhenMapperThrows_ShouldReturnFailureResult()
        {
            // Arrange
            var result = Result<int>.Ok(5);
            var mapperException = new InvalidOperationException("FlatMapper failed");

            // Act
            var flatMapped = result.FlatMap<string>(x => throw mapperException);

            // Assert
            await Assert.That(flatMapped.Success).IsFalse();
            await Assert.That(flatMapped.Exception).IsSameReferenceAs(mapperException);
        }

        #endregion

        #region FlatMapAsync Tests

        [Test]
        public async Task FlatMapAsync_OnSuccessResult_ShouldChainOperations()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act
            var flatMapped = await result.FlatMapAsync(async x => 
            {
                await Task.Delay(10);
                return Result<string>.Ok($"Value: {x}");
            });

            // Assert
            await Assert.That(flatMapped.Success).IsTrue();
            await Assert.That(flatMapped.Data).IsEqualTo("Value: 5");
        }

        [Test]
        public async Task FlatMapAsync_OnFailureResult_ShouldPropagateFailure()
        {
            // Arrange
            var result = Result<int>.Fail("Failed");

            // Act
            var flatMapped = await result.FlatMapAsync(async x => 
            {
                await Task.Delay(10);
                return Result<string>.Ok($"Value: {x}");
            });

            // Assert
            await Assert.That(flatMapped.Success).IsFalse();
            await Assert.That(flatMapped.Message).IsEqualTo("Failed");
        }

        [Test]
        public async Task FlatMapAsync_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Ok(5);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await result.FlatMapAsync<string>(null)
            );
        }

        #endregion

        #region MapError Tests for Result

        [Test]
        public async Task MapError_OnFailureResult_ShouldTransformException()
        {
            // Arrange
            var originalException = new InvalidOperationException("Original error");
            var result = Result.Fail("Failed", originalException);

            // Act
            var mapped = result.MapError(ex => new ArgumentException("Transformed error", ex));

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsTypeOf<ArgumentException>();
            await Assert.That(mapped.Exception.Message).Contains("Transformed error");
            await Assert.That(mapped.Exception.InnerException).IsSameReferenceAs(originalException);
        }

        [Test]
        public async Task MapError_OnSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = Result.Ok("Success");

            // Act
            var mapped = result.MapError(ex => new ArgumentException("Should not be called"));

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Message).IsEqualTo("Success");
            await Assert.That(mapped.Exception).IsNull();
        }

        [Test]
        public async Task MapError_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result.Fail("Failed");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                Task.FromResult(result.MapError(null)));
        }

        [Test]
        public async Task MapError_WhenMapperReturnsNull_ShouldKeepNullException()
        {
            // Arrange
            var result = Result.Fail("Failed without exception");

            // Act
            var mapped = result.MapError(ex => null);

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsNull();
            await Assert.That(mapped.Message).IsEqualTo("Failed without exception");
        }

        #endregion

        #region MapError Tests for Result<T>

        [Test]
        public async Task MapErrorGeneric_OnFailureResult_ShouldTransformException()
        {
            // Arrange
            var originalException = new InvalidOperationException("Original error");
            var result = Result<int>.Fail("Failed", originalException);

            // Act
            var mapped = result.MapError(ex => new ArgumentException("Transformed error", ex));

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsTypeOf<ArgumentException>();
            await Assert.That(mapped.Exception.Message).Contains("Transformed error");
            await Assert.That(mapped.Exception.InnerException).IsSameReferenceAs(originalException);
        }

        [Test]
        public async Task MapErrorGeneric_OnSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = Result<int>.Ok(42);

            // Act
            var mapped = result.MapError(ex => new ArgumentException("Should not be called"));

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Data).IsEqualTo(42);
            await Assert.That(mapped.Exception).IsNull();
        }

        [Test]
        public async Task MapErrorGeneric_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Fail("Failed");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                Task.FromResult(result.MapError(null)));
        }

        #endregion

        #region MapErrorAsync Tests

        [Test]
        public async Task MapErrorAsync_OnFailureResult_ShouldTransformException()
        {
            // Arrange
            var originalException = new InvalidOperationException("Original error");
            var result = Result.Fail("Failed", originalException);

            // Act
            var mapped = await result.MapErrorAsync(async ex => 
            {
                await Task.Delay(10);
                return new ArgumentException("Async transformed error", ex);
            });

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsTypeOf<ArgumentException>();
            await Assert.That(mapped.Exception.Message).Contains("Async transformed error");
        }

        [Test]
        public async Task MapErrorAsync_OnSuccessResult_ShouldReturnSuccess()
        {
            // Arrange
            var result = Result.Ok("Success");

            // Act
            var mapped = await result.MapErrorAsync(async ex => 
            {
                await Task.Delay(10);
                return new ArgumentException("Should not be called");
            });

            // Assert
            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Message).IsEqualTo("Success");
        }

        [Test]
        public async Task MapErrorAsyncGeneric_OnFailureResult_ShouldTransformException()
        {
            // Arrange
            var originalException = new InvalidOperationException("Original error");
            var result = Result<int>.Fail("Failed", originalException);

            // Act
            var mapped = await result.MapErrorAsync(async ex => 
            {
                await Task.Delay(10);
                return new ArgumentException("Async transformed error", ex);
            });

            // Assert
            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Exception).IsTypeOf<ArgumentException>();
        }

        [Test]
        public async Task MapErrorAsyncGeneric_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int>.Fail("Failed");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await result.MapErrorAsync(null)
            );
        }

        #endregion

        #region Real-World Scenario Tests

        [Test]
        public async Task Match_InLinqQuery_ShouldWorkCorrectly()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(1),
                Result<int>.Fail("Error"),
                Result<int>.Ok(3)
            };

            // Act
            var outputs = results
                .Select(r => r.Match(
                    onSuccess: v => v.ToString(),
                    onFailure: ex => "N/A"
                ))
                .ToList();

            // Assert
            await Assert.That(outputs).IsEquivalentTo(new[] { "1", "N/A", "3" });
        }

        [Test]
        public async Task FunctionalChaining_ShouldComposeCorrectly()
        {
            // Arrange
            Result<int> ParseInt(string s) => 
                int.TryParse(s, out var result) 
                    ? Result<int>.Ok(result) 
                    : Result<int>.Fail($"Cannot parse '{s}' as integer");

            Result<double> Divide(int numerator, int denominator) =>
                denominator != 0
                    ? Result<double>.Ok((double)numerator / denominator)
                    : Result<double>.Fail("Cannot divide by zero");

            // Act
            var result = ParseInt("10")
                .FlatMap(x => Divide(x, 2))
                .Map(x => Math.Round(x, 2))
                .Match(
                    onSuccess: value => $"Result: {value}",
                    onFailure: ex => $"Error: {ex?.Message ?? "Unknown error"}"
                );

            // Assert
            await Assert.That(result).IsEqualTo("Result: 5");
        }

        [Test]
        public async Task ComplexTypeTransformation_ShouldWorkCorrectly()
        {
            // Arrange
            var userResult = Result<User>.Ok(new User { Id = 1, Name = "John Doe" });

            // Act
            var apiResponse = userResult
                .Map(user => new UserDto { UserId = user.Id, DisplayName = user.Name.ToUpper() })
                .MapError(ex => new InvalidOperationException("Failed to transform user", ex))
                .Match(
                    onSuccess: dto => new ApiResponse { StatusCode = 200, Data = dto },
                    onFailure: ex => new ApiResponse { StatusCode = 500, ErrorMessage = ex.Message }
                );

            // Assert
            await Assert.That(apiResponse.StatusCode).IsEqualTo(200);
            await Assert.That(((UserDto)apiResponse.Data).DisplayName).IsEqualTo("JOHN DOE");
        }

        #endregion

        #region Helper Classes

        private class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class UserDto
        {
            public int UserId { get; set; }
            public string DisplayName { get; set; }
        }

        private class ApiResponse
        {
            public int StatusCode { get; set; }
            public object Data { get; set; }
            public string ErrorMessage { get; set; }
        }

        #endregion
    }
}