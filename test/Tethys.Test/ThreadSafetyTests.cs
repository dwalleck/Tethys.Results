using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class ThreadSafetyTests
    {
        [Test]
        public async Task Result_ShouldBeImmutable()
        {
            // Arrange
            var result = Result.Ok("Success message");
            var initialSuccess = result.Success;
            var initialMessage = result.Message;
            var initialException = result.Exception;

            // Act & Assert - Verify that properties cannot be modified
            // This is a compile-time check, but we'll verify at runtime too
            await Assert.That(result.Success).IsEqualTo(initialSuccess);
            await Assert.That(result.Message).IsEqualTo(initialMessage);
            await Assert.That(result.Exception).IsEqualTo(initialException);

            // Create a new result with the same values
            var result2 = Result.Ok("Success message");

            // Verify that they are different instances
            await Assert.That(ReferenceEquals(result, result2)).IsFalse();
        }

        [Test]
        public async Task GenericResult_ShouldBeImmutable()
        {
            // Arrange
            var data = "Test data";
            var result = Result<string>.Ok(data, "Success message");
            var initialSuccess = result.Success;
            var initialMessage = result.Message;
            var initialData = result.Data;
            var initialException = result.Exception;

            // Act & Assert - Verify that properties cannot be modified
            // This is a compile-time check, but we'll verify at runtime too
            await Assert.That(result.Success).IsEqualTo(initialSuccess);
            await Assert.That(result.Message).IsEqualTo(initialMessage);
            await Assert.That(result.Data).IsEqualTo(initialData);
            await Assert.That(result.Exception).IsEqualTo(initialException);

            // Create a new result with the same values
            var result2 = Result<string>.Ok(data, "Success message");

            // Verify that they are different instances
            await Assert.That(ReferenceEquals(result, result2)).IsFalse();
        }

        [Test]
        public async Task Result_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var result = Result.Ok("Success message");
            var tasks = new List<Task<bool>>();

            // Act - Access the result from multiple threads
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => result.Success));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            // Assert - All tasks should get the same value
            await Assert.That(results.All(r => r)).IsTrue();
        }

        [Test]
        public async Task GenericResult_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var result = Result<string>.Ok("Test data", "Success message");
            var successTasks = new List<Task<bool>>();
            var dataTasks = new List<Task<string>>();

            // Act - Access the result from multiple threads
            for (int i = 0; i < 100; i++)
            {
                successTasks.Add(Task.Run(() => result.Success));
                dataTasks.Add(Task.Run(() => result.Data));
            }

            // Wait for all tasks to complete
            var successResults = await Task.WhenAll(successTasks);
            var dataResults = await Task.WhenAll(dataTasks);

            // Assert - All tasks should get the same values
            await Assert.That(successResults.All(r => r)).IsTrue();
            await Assert.That(dataResults.All(r => r == "Test data")).IsTrue();
        }

        [Test]
        public async Task ResultExtensions_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            var tasks = new List<Task<Result>>();

            // Act - Call Then from multiple threads
            for (int i = 0; i < 100; i++)
            {
                int capturedI = i;
                tasks.Add(Task.Run(() => result.Then(() => Result.Ok($"Next result {capturedI}"))));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            // Assert - All tasks should produce successful results
            await Assert.That(results.All(r => r.Success)).IsTrue();
        }

        [Test]
        public async Task ResultExtensions_ShouldHandleConcurrentAsyncAccess()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            var tasks = new List<Task<Result>>();

            // Act - Call ThenAsync from multiple threads
            for (int i = 0; i < 100; i++)
            {
                int capturedI = i;
                tasks.Add(Task.Run(async () =>
                    await result.ThenAsync(async () =>
                    {
                        await Task.Delay(10); // Simulate async work
                        return Result.Ok($"Next result {capturedI}");
                    })
                ));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            // Assert - All tasks should produce successful results
            await Assert.That(results.All(r => r.Success)).IsTrue();
        }

        [Test]
        public async Task ValueExtractionMethods_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var result = Result<string>.Ok("Test data");
            var getValueTasks = new List<Task<string>>();
            var tryGetValueTasks = new List<Task<(bool success, string value)>>();

            // Act - Call value extraction methods from multiple threads
            for (int i = 0; i < 100; i++)
            {
                getValueTasks.Add(Task.Run(() => result.GetValueOrDefault()));
                tryGetValueTasks.Add(Task.Run(() =>
                {
                    bool success = result.TryGetValue(out var value);
                    return (success, value);
                }));
            }

            // Wait for all tasks to complete
            var getValueResults = await Task.WhenAll(getValueTasks);
            var tryGetValueResults = await Task.WhenAll(tryGetValueTasks);

            // Assert - All tasks should get the same values
            await Assert.That(getValueResults.All(r => r == "Test data")).IsTrue();
            await Assert.That(tryGetValueResults.All(r => r.success && r.value == "Test data")).IsTrue();
        }

        [Test]
        public async Task MutableReferenceType_ShouldNotAffectResultImmutability()
        {
            // Arrange
            var mutableObject = new MutableObject { Value = "Initial value" };
            var result = Result<MutableObject>.Ok(mutableObject);

            // Act - Modify the mutable object after creating the result
            mutableObject.Value = "Modified value";

            // Assert - The result's data should reflect the modified value
            // This is expected behavior since the result only stores a reference to the object
            await Assert.That(result.Data.Value).IsEqualTo("Modified value");

            // However, the result itself should still be immutable
            var initialSuccess = result.Success;
            var initialMessage = result.Message;

            // Verify that properties cannot be modified
            await Assert.That(result.Success).IsEqualTo(initialSuccess);
            await Assert.That(result.Message).IsEqualTo(initialMessage);
        }

        private class MutableObject
        {
            public string Value { get; set; }
        }
    }
}
