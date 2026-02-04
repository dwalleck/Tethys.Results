using System;
using System.Threading.Tasks;
using Tethys.Results;

namespace Tethys.Test
{
    public class TypedResultTests
    {
        [Test]
        public async Task Ok_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var value = "test value";

            // Act
            var result = Result<string, string>.Ok(value);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(value);
        }

        [Test]
        public async Task Fail_ShouldCreateFailedResult()
        {
            // Arrange
            var error = "validation error";

            // Act
            var result = Result<string, string>.Fail(error);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(error);
        }

        [Test]
        public async Task Value_OnFailedResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var result = Result<string, string>.Fail("error");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                _ = result.Value;
                return Task.CompletedTask;
            });
            await Assert.That(exception.Message).IsEqualTo("Cannot access Value on a failed result. Check Success first or use Match().");
        }

        [Test]
        public async Task Error_OnSuccessfulResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var result = Result<string, string>.Ok("value");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                _ = result.Error;
                return Task.CompletedTask;
            });
            await Assert.That(exception.Message).IsEqualTo("Cannot access Error on a successful result. Check Success first or use Match().");
        }

        [Test]
        public async Task Match_OnSuccess_ShouldExecuteOnSuccessFunction()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act
            var output = result.Match(
                onSuccess: value => $"Value: {value}",
                onFailure: error => $"Error: {error}"
            );

            // Assert
            await Assert.That(output).IsEqualTo("Value: 42");
        }

        [Test]
        public async Task Match_OnFailure_ShouldExecuteOnFailureFunction()
        {
            // Arrange
            var result = Result<int, string>.Fail("not found");

            // Act
            var output = result.Match(
                onSuccess: value => $"Value: {value}",
                onFailure: error => $"Error: {error}"
            );

            // Assert
            await Assert.That(output).IsEqualTo("Error: not found");
        }

        [Test]
        public async Task Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                _ = result.Match<string>(null!, error => error);
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task Match_WithNullOnFailure_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                _ = result.Match<string>(value => value.ToString(), null!);
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task MatchAsync_OnSuccess_ShouldExecuteOnSuccessFunction()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act
            var output = await result.MatchAsync(
                onSuccess: async value => { await Task.Delay(1); return $"Value: {value}"; },
                onFailure: async error => { await Task.Delay(1); return $"Error: {error}"; }
            );

            // Assert
            await Assert.That(output).IsEqualTo("Value: 42");
        }

        [Test]
        public async Task MatchAsync_OnFailure_ShouldExecuteOnFailureFunction()
        {
            // Arrange
            var result = Result<int, string>.Fail("not found");

            // Act
            var output = await result.MatchAsync(
                onSuccess: async value => { await Task.Delay(1); return $"Value: {value}"; },
                onFailure: async error => { await Task.Delay(1); return $"Error: {error}"; }
            );

            // Assert
            await Assert.That(output).IsEqualTo("Error: not found");
        }

        [Test]
        public async Task Equals_TwoSuccessfulResultsWithSameValue_ShouldBeEqual()
        {
            // Arrange
            var result1 = Result<int, string>.Ok(42);
            var result2 = Result<int, string>.Ok(42);

            // Assert
            await Assert.That(result1.Equals(result2)).IsTrue();
            await Assert.That(result1 == result2).IsTrue();
            await Assert.That(result1 != result2).IsFalse();
        }

        [Test]
        public async Task Equals_TwoFailedResultsWithSameError_ShouldBeEqual()
        {
            // Arrange
            var result1 = Result<int, string>.Fail("error");
            var result2 = Result<int, string>.Fail("error");

            // Assert
            await Assert.That(result1.Equals(result2)).IsTrue();
            await Assert.That(result1 == result2).IsTrue();
        }

        [Test]
        public async Task Equals_SuccessAndFailure_ShouldNotBeEqual()
        {
            // Arrange
            var success = Result<int, string>.Ok(42);
            var failure = Result<int, string>.Fail("error");

            // Assert
            await Assert.That(success.Equals(failure)).IsFalse();
            await Assert.That(success == failure).IsFalse();
            await Assert.That(success != failure).IsTrue();
        }

        [Test]
        public async Task Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Assert
            await Assert.That(result.Equals(null)).IsFalse();
            await Assert.That(result == null).IsFalse();
            await Assert.That(null == result).IsFalse();
        }

        [Test]
        public async Task GetHashCode_EqualResults_ShouldHaveSameHashCode()
        {
            // Arrange
            var result1 = Result<int, string>.Ok(42);
            var result2 = Result<int, string>.Ok(42);

            // Assert
            await Assert.That(result1.GetHashCode()).IsEqualTo(result2.GetHashCode());
        }

        [Test]
        public async Task Ok_WithValueType_ShouldWork()
        {
            // Arrange & Act
            var result = Result<int, int>.Ok(42);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(42);
        }

        [Test]
        public async Task Fail_WithValueType_ShouldWork()
        {
            // Arrange & Act
            var result = Result<int, int>.Fail(500);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(500);
        }

        [Test]
        public async Task Ok_WithNullValue_ShouldWork()
        {
            // Arrange & Act
            var result = Result<string, string>.Ok(null);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsNull();
        }

        [Test]
        public async Task Fail_WithNullError_ShouldWork()
        {
            // Arrange & Act
            var result = Result<string, string>.Fail(null);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsNull();
        }

        [Test]
        public async Task Match_WithComplexTypes_ShouldWork()
        {
            // Arrange
            var result = Result<DateTime, Exception>.Ok(new DateTime(2026, 1, 1));

            // Act
            var year = result.Match(
                onSuccess: date => date.Year,
                onFailure: ex => -1
            );

            // Assert
            await Assert.That(year).IsEqualTo(2026);
        }

        [Test]
        public async Task MatchAsync_WithNullOnSuccess_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await result.MatchAsync<string>(null!, async error => await Task.FromResult(error));
            });
        }

        [Test]
        public async Task MatchAsync_WithNullOnFailure_ShouldThrowArgumentNullException()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await result.MatchAsync<string>(async value => await Task.FromResult(value.ToString()), null!);
            });
        }

        [Test]
        public async Task GetHashCode_EqualFailedResults_ShouldHaveSameHashCode()
        {
            // Arrange
            var result1 = Result<int, string>.Fail("error");
            var result2 = Result<int, string>.Fail("error");

            // Assert
            await Assert.That(result1.GetHashCode()).IsEqualTo(result2.GetHashCode());
        }

        [Test]
        public async Task Equals_TwoSuccessfulResultsWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var result1 = Result<int, string>.Ok(42);
            var result2 = Result<int, string>.Ok(99);

            // Assert
            await Assert.That(result1.Equals(result2)).IsFalse();
            await Assert.That(result1 == result2).IsFalse();
            await Assert.That(result1 != result2).IsTrue();
        }

        [Test]
        public async Task Equals_TwoFailedResultsWithDifferentErrors_ShouldNotBeEqual()
        {
            // Arrange
            var result1 = Result<int, string>.Fail("error1");
            var result2 = Result<int, string>.Fail("error2");

            // Assert
            await Assert.That(result1.Equals(result2)).IsFalse();
            await Assert.That(result1 == result2).IsFalse();
            await Assert.That(result1 != result2).IsTrue();
        }

        [Test]
        public async Task EqualsObject_WithEqualResult_ShouldReturnTrue()
        {
            // Arrange
            var result1 = Result<int, string>.Ok(42);
            object result2 = Result<int, string>.Ok(42);

            // Assert
            await Assert.That(result1.Equals(result2)).IsTrue();
        }

        [Test]
        public async Task EqualsObject_WithDifferentType_ShouldReturnFalse()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);
            object notAResult = "not a result";

            // Assert
            await Assert.That(result.Equals(notAResult)).IsFalse();
        }

        [Test]
        public async Task Equals_SameReference_ShouldReturnTrue()
        {
            // Arrange
            var result = Result<int, string>.Ok(42);

            // Assert
            await Assert.That(result.Equals(result)).IsTrue();
        }
    }
}
