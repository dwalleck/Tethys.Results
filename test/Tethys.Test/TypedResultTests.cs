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
    }
}
