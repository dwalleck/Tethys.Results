// Example test file demonstrating TDD approach for Match feature
// This would be created BEFORE implementing the Match method

using TUnit.Core;
using Tethys.Results;
using System;
using System.Threading.Tasks;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for the Match pattern matching feature.
    /// Written BEFORE implementation following TDD principles.
    /// </summary>
    public class MatchTests
    {
        #region Basic Match Tests for Result

        [Test]
        public void Match_OnSuccessResult_ShouldCallOnSuccessFunction()
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
            Assert.That(onSuccessCalled, Is.True);
            Assert.That(onFailureCalled, Is.False);
            Assert.That(output, Is.EqualTo("Success branch"));
        }

        [Test]
        public void Match_OnFailureResult_ShouldCallOnFailureFunction()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result.Fail(exception);
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
            Assert.That(onSuccessCalled, Is.False);
            Assert.That(onFailureCalled, Is.True);
            Assert.That(output, Is.EqualTo("Failure branch"));
            Assert.That(capturedError, Is.SameAs(exception));
        }

        #endregion

        #region Basic Match Tests for Result<T>

        [Test]
        public void MatchGeneric_OnSuccessResult_ShouldCallOnSuccessWithValue()
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
            Assert.That(onSuccessCalled, Is.True);
            Assert.That(onFailureCalled, Is.False);
            Assert.That(capturedValue, Is.EqualTo(42));
            Assert.That(output, Is.EqualTo("Value is 42"));
        }

        [Test]
        public void MatchGeneric_OnFailureResult_ShouldCallOnFailureFunction()
        {
            // Arrange
            var exception = new ArgumentException("Invalid argument");
            var result = Result<string>.Fail(exception);
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
                    return ex.Message;
                }
            );

            // Assert
            Assert.That(onSuccessCalled, Is.False);
            Assert.That(onFailureCalled, Is.True);
            Assert.That(output, Is.EqualTo("Invalid argument"));
        }

        #endregion

        #region Null Parameter Tests

        [Test]
        public void Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result.Ok();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.Match<string>(
                    onSuccess: null,
                    onFailure: (ex) => "failure"
                )
            );
        }

        [Test]
        public void Match_WithNullOnFailure_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result.Ok();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                result.Match<string>(
                    onSuccess: () => "success",
                    onFailure: null
                )
            );
        }

        #endregion

        #region Complex Type Tests

        [Test]
        public void Match_WithComplexReturnType_ShouldWorkCorrectly()
        {
            // Arrange
            var result = Result<User>.Ok(new User { Id = 1, Name = "John" });

            // Act
            var response = result.Match(
                onSuccess: (user) => new ApiResponse 
                { 
                    StatusCode = 200, 
                    Data = user,
                    Message = "User found"
                },
                onFailure: (ex) => new ApiResponse 
                { 
                    StatusCode = 404, 
                    Data = null,
                    Message = ex.Message
                }
            );

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Data.Name, Is.EqualTo("John"));
            Assert.That(response.Message, Is.EqualTo("User found"));
        }

        #endregion

        #region Async Match Tests

        [Test]
        public async Task MatchAsync_OnSuccessResult_ShouldAwaitOnSuccessFunction()
        {
            // Arrange
            var result = Result<int>.Ok(10);
            var onSuccessCalled = false;

            // Act
            var output = await result.MatchAsync(
                onSuccess: async (value) => 
                {
                    await Task.Delay(10); // Simulate async work
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
            Assert.That(onSuccessCalled, Is.True);
            Assert.That(output, Is.EqualTo(20));
        }

        [Test]
        public async Task MatchAsync_OnFailureResult_ShouldAwaitOnFailureFunction()
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
            Assert.That(onFailureCalled, Is.True);
            Assert.That(output, Is.EqualTo(-1));
        }

        #endregion

        #region Real-World Scenario Tests

        [Test]
        public void Match_InLinqQuery_ShouldWorkCorrectly()
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
            Assert.That(outputs, Is.EquivalentTo(new[] { "1", "N/A", "3" }));
        }

        [Test]
        public void Match_WithNestedResults_ShouldComposeCorrectly()
        {
            // Arrange
            Result<int> GetNumber() => Result<int>.Ok(5);
            Result<int> Double(int n) => Result<int>.Ok(n * 2);

            // Act
            var output = GetNumber().Match(
                onSuccess: (n) => Double(n).Match(
                    onSuccess: (doubled) => $"Result: {doubled}",
                    onFailure: (ex) => "Double failed"
                ),
                onFailure: (ex) => "GetNumber failed"
            );

            // Assert
            Assert.That(output, Is.EqualTo("Result: 10"));
        }

        #endregion

        #region Test Helper Classes

        private class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class ApiResponse
        {
            public int StatusCode { get; set; }
            public User Data { get; set; }
            public string Message { get; set; }
        }

        #endregion
    }
}