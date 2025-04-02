﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class ResultPatternTests
    {
        [Test]
        public async Task Result_Ok_ShouldReturnSuccessfulResult()
        {
            // Act
            var result = Result.Ok("Success message");

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Success message");
            await Assert.That(result.Exception).IsNull();
        }


        [Test]
        public async Task Result_Fail_ShouldReturnFailedResult()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act

            var result = Result.Fail("Error message", exception);

            // Assert

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Error message");
            await Assert.That(result.Exception).IsEqualTo(exception);
        }


        [Test]
        public async Task GenericResult_Ok_ShouldReturnSuccessfulResultWithData()
        {
            // Arrange
            var testData = "Test data";

            // Act

            var result = Result<string>.Ok(testData, "Success message");

            // Assert

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Success message");
            await Assert.That(result.Data).IsEqualTo(testData);
            await Assert.That(result.Exception).IsNull();
        }


        [Test]
        public async Task GenericResult_Fail_ShouldReturnFailedResultWithoutData()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act

            var result = Result<string>.Fail("Error message", exception);

            // Assert

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Error message");
            await Assert.That(result.Data).IsEqualTo(default(string));
            await Assert.That(result.Exception).IsEqualTo(exception);
        }


        [Test]
        public async Task Then_ShouldExecuteNextOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result.Ok("First operation");
            bool nextOperationExecuted = false;

            // Act

            var finalResult = result.Then(() =>
            {
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task Then_ShouldNotExecuteNextOperation_WhenPreviousFailed()
        {
            // Arrange
            var result = Result.Fail("First operation failed");
            bool nextOperationExecuted = false;

            // Act

            var finalResult = result.Then(() =>
            {
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(nextOperationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("First operation failed");
        }


        [Test]
        public async Task Then_WithData_ShouldPassDataToNextOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");
            int passedValue = 0;

            // Act

            var finalResult = result.Then(value =>
            {
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task Then_WithData_ShouldTransformData_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");

            // Act

            var finalResult = result.Then(value =>

                Result<string>.Ok(value.ToString(), "Transformed data")
            );

            // Assert

            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Data).IsEqualTo("42");
            await Assert.That(finalResult.Message).IsEqualTo("Transformed data");
        }


        [Test]
        public async Task When_ShouldExecuteOperation_WhenConditionIsTrue()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            bool operationExecuted = false;

            // Act

            var finalResult = result.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert

            await Assert.That(operationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Conditional operation");
        }


        [Test]
        public async Task When_ShouldNotExecuteOperation_WhenConditionIsFalse()
        {
            // Arrange
            var result = Result.Ok("Initial result");
            bool operationExecuted = false;

            // Act

            var finalResult = result.When(false, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert

            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Initial result");
        }


        [Test]
        public async Task When_ShouldNotExecuteOperation_WhenPreviousResultFailed()
        {
            // Arrange
            var result = Result.Fail("Initial failure");
            bool operationExecuted = false;

            // Act

            var finalResult = result.When(true, () =>
            {
                operationExecuted = true;
                return Result.Ok("Conditional operation");
            });

            // Assert

            await Assert.That(operationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("Initial failure");
        }


        [Test]
        public async Task ComplexPipeline_ShouldExecuteAllOperations_AndReturnFinalResult()
        {
            // Arrange
            var executionOrder = new System.Text.StringBuilder();

            // Act

            var result = Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .When(true, () =>
                {
                    executionOrder.Append("2-");
                    return Result.Ok("Conditional operation");
                })
                .Then(() =>
                {
                    executionOrder.Append("3");
                    return Result.Ok("Final operation");
                });

            // Assert

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }


        [Test]
        public async Task ComplexPipeline_ShouldStopAtFirstFailure()
        {
            // Arrange
            var executionOrder = new System.Text.StringBuilder();

            // Act

            var result = Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .Then(() =>
                {
                    executionOrder.Append("2-");
                    return Result.Fail("Operation failed");
                })
                .Then(() =>
                {
                    executionOrder.Append("3");
                    return Result.Ok("This should not execute");
                });

            // Assert

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Operation failed");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-");
        }

        #region Async Tests


        [Test]
        public async Task ThenAsync_ShouldExecuteNextAsyncOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result.Ok("First operation");
            bool nextOperationExecuted = false;

            // Act

            var finalResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task ThenAsync_ShouldNotExecuteNextAsyncOperation_WhenPreviousFailed()
        {
            // Arrange
            var result = Result.Fail("First operation failed");
            bool nextOperationExecuted = false;

            // Act

            var finalResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(nextOperationExecuted).IsFalse();
            await Assert.That(finalResult.Success).IsFalse();
            await Assert.That(finalResult.Message).IsEqualTo("First operation failed");
        }


        [Test]
        public async Task ThenAsync_WithData_ShouldPassDataToNextAsyncOperation_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");
            int passedValue = 0;

            // Act

            var finalResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task ThenAsync_WithData_ShouldTransformDataAsynchronously_WhenPreviousSucceeded()
        {
            // Arrange
            var result = Result<int>.Ok(42, "First operation");

            // Act

            var finalResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                return Result<string>.Ok(value.ToString(), "Transformed data");
            });

            // Assert

            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Data).IsEqualTo("42");
            await Assert.That(finalResult.Message).IsEqualTo("Transformed data");
        }


        [Test]
        public async Task ThenAsync_WithTaskResult_ShouldChainAsyncOperations()
        {
            // Arrange
            Task<Result> asyncResult = Task.FromResult(Result.Ok("First operation"));
            bool nextOperationExecuted = false;

            // Act

            var finalResult = await asyncResult.ThenAsync(async () =>
            {
                await Task.Delay(10); // Simulate async work
                nextOperationExecuted = true;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(nextOperationExecuted).IsTrue();
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task ThenAsync_WithTaskGenericResult_ShouldChainAsyncOperationsWithData()
        {
            // Arrange
            Task<Result<int>> asyncResult = Task.FromResult(Result<int>.Ok(42, "First operation"));
            int passedValue = 0;

            // Act

            var finalResult = await asyncResult.ThenAsync(async value =>
            {
                await Task.Delay(10); // Simulate async work
                passedValue = value;
                return Result.Ok("Second operation");
            });

            // Assert

            await Assert.That(passedValue).IsEqualTo(42);
            await Assert.That(finalResult.Success).IsTrue();
            await Assert.That(finalResult.Message).IsEqualTo("Second operation");
        }


        [Test]
        public async Task ComplexAsyncPipeline_ShouldExecuteAllOperationsAsynchronously()
        {
            // Arrange
            var executionOrder = new System.Text.StringBuilder();

            // Act

            var result = await Result.Ok("Start")
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Ok("Second operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("Final operation");
                });

            // Assert

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }


        [Test]
        public async Task ComplexAsyncPipeline_ShouldStopAtFirstFailure()
        {
            // Arrange
            var executionOrder = new System.Text.StringBuilder();

            // Act

            var result = await Result.Ok("Start")
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("1-");
                    return Result.Ok("First operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Fail("Operation failed");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("This should not execute");
                });

            // Assert

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Message).IsEqualTo("Operation failed");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-");
        }


        [Test]
        public async Task MixedSyncAndAsyncPipeline_ShouldWorkCorrectly()
        {
            // Arrange
            var executionOrder = new System.Text.StringBuilder();

            // Act

            var result = await Result.Ok("Start")
                .Then(() =>
                {
                    executionOrder.Append("1-");
                    return Result.Ok("Sync operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("2-");
                    return Result.Ok("Async operation");
                })
                .ThenAsync(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    executionOrder.Append("3");
                    return Result.Ok("Final sync operation");
                });

            // Assert

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Message).IsEqualTo("Final sync operation");
            await Assert.That(executionOrder.ToString()).IsEqualTo("1-2-3");
        }

        #endregion

        #region Value Extraction Tests


        [Test]
        public async Task GetValueOrDefault_ShouldReturnValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act

            var value = result.GetValueOrDefault();

            // Assert

            await Assert.That(value).IsEqualTo(expectedValue);
        }


        [Test]
        public async Task GetValueOrDefault_ShouldReturnDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<string>.Fail("Operation failed");

            // Act

            var value = result.GetValueOrDefault();

            // Assert

            await Assert.That(value).IsEqualTo(default(string));
        }


        [Test]
        public async Task GetValueOrDefault_ShouldReturnCustomDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var customDefault = "Custom default";
            var result = Result<string>.Fail("Operation failed");

            // Act

            var value = result.GetValueOrDefault(customDefault);

            // Assert

            await Assert.That(value).IsEqualTo(customDefault);
        }


        [Test]
        public async Task GetValueOrDefault_ShouldWorkWithValueTypes()
        {
            // Arrange
            var expectedValue = 42;
            var result = Result<int>.Ok(expectedValue);

            // Act

            var value = result.GetValueOrDefault();

            // Assert

            await Assert.That(value).IsEqualTo(expectedValue);
        }


        [Test]
        public async Task GetValueOrDefault_ShouldReturnCustomDefaultForValueTypes_WhenResultIsFailed()
        {
            // Arrange
            var customDefault = 99;
            var result = Result<int>.Fail("Operation failed");

            // Act

            var value = result.GetValueOrDefault(customDefault);

            // Assert

            await Assert.That(value).IsEqualTo(customDefault);
        }


        [Test]
        public async Task TryGetValue_ShouldReturnTrueAndSetValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act

            bool success = result.TryGetValue(out var value);

            // Assert

            await Assert.That(success).IsTrue();
            await Assert.That(value).IsEqualTo(expectedValue);
        }


        [Test]
        public async Task TryGetValue_ShouldReturnFalseAndSetDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<string>.Fail("Operation failed");

            // Act

            bool success = result.TryGetValue(out var value);

            // Assert

            await Assert.That(success).IsFalse();
            await Assert.That(value).IsEqualTo(default(string));
        }


        [Test]
        public async Task TryGetValue_ShouldWorkWithValueTypes()
        {
            // Arrange
            var expectedValue = 42;
            var result = Result<int>.Ok(expectedValue);

            // Act

            bool success = result.TryGetValue(out var value);

            // Assert

            await Assert.That(success).IsTrue();
            await Assert.That(value).IsEqualTo(expectedValue);
        }


        [Test]
        public async Task TryGetValue_ShouldSetDefaultForValueTypes_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Operation failed");

            // Act

            bool success = result.TryGetValue(out var value);

            // Assert

            await Assert.That(success).IsFalse();
            await Assert.That(value).IsEqualTo(default(int));
        }


        [Test]
        public async Task GetValueOrThrow_ShouldReturnValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act

            var value = result.GetValueOrThrow();

            // Assert

            await Assert.That(value).IsEqualTo(expectedValue);
        }


        [Test]
        public async Task GetValueOrThrow_ShouldThrowException_WhenResultIsFailed()
        {
            // Arrange
            var errorMessage = "Operation failed";
            var result = Result<string>.Fail(errorMessage);

            // Act & Assert

            try
            {
                var value = result.GetValueOrThrow();
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                await Assert.That(ex.Message).IsEqualTo(errorMessage);
            }
        }


        [Test]
        public async Task GetValueOrThrow_ShouldThrowOriginalException_WhenResultIsFailedWithException()
        {
            // Arrange
            var originalException = new ArgumentException("Invalid argument");
            var result = Result<string>.Fail("Operation failed", originalException);

            // Act & Assert

            try
            {
                var value = result.GetValueOrThrow();
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                await Assert.That(ex).IsEqualTo(originalException);
            }
        }


        [Test]
        public async Task ValueExtractionMethods_ShouldWorkWithReferenceTypes()
        {
            // Arrange
            var expectedObject = new TestObject { Id = 1, Name = "Test" };
            var result = Result<TestObject>.Ok(expectedObject);

            // Act & Assert
            // GetValueOrDefault

            var value1 = result.GetValueOrDefault();
            await Assert.That(value1).IsEqualTo(expectedObject);

            // TryGetValue

            bool success = result.TryGetValue(out var value2);
            await Assert.That(success).IsTrue();
            await Assert.That(value2).IsEqualTo(expectedObject);

            // GetValueOrThrow

            var value3 = result.GetValueOrThrow();
            await Assert.That(value3).IsEqualTo(expectedObject);
        }


        [Test]
        public async Task ValueExtractionMethods_ShouldWorkWithNullableValueTypes()
        {
            // Arrange
            int? expectedValue = 42;
            var result = Result<int?>.Ok(expectedValue);

            // Act & Assert
            // GetValueOrDefault

            var value1 = result.GetValueOrDefault();
            await Assert.That(value1).IsEqualTo(expectedValue);

            // With custom default

            var failedResult = Result<int?>.Fail("Failed");
            var value2 = failedResult.GetValueOrDefault(99);
            await Assert.That(value2).IsEqualTo(99);

            // TryGetValue

            bool success = result.TryGetValue(out var value3);
            await Assert.That(success).IsTrue();
            await Assert.That(value3).IsEqualTo(expectedValue);

            // GetValueOrThrow

            var value4 = result.GetValueOrThrow();
            await Assert.That(value4).IsEqualTo(expectedValue);
        }


        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }


            public override bool Equals(object obj)
            {
                if (obj is TestObject other)
                {
                    return Id == other.Id && Name == other.Name;
                }
                return false;
            }


            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Name);
            }
        }

        #endregion

        #region Thread Safety Tests


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


    #endregion
    
    #region Input Validation Tests
    
    [Test]
    public async Task Ok_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result.Ok(null);
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            await Assert.That(ex.ParamName).IsEqualTo("message");
        }
    }
    
    [Test]
    public async Task GenericOk_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result<string>.Ok("data", null);
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            await Assert.That(ex.ParamName).IsEqualTo("message");
        }
    }
    
    [Test]
    public async Task Fail_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result.Fail(null);
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            await Assert.That(ex.ParamName).IsEqualTo("message");
        }
    }
    
    [Test]
    public async Task GenericFail_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result<string>.Fail(null);
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            await Assert.That(ex.ParamName).IsEqualTo("message");
        }
    }
    
    [Test]
    public async Task FromException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result.FromException(null);
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
    public async Task GenericFromException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
    {
        // Act & Assert
        try
        {
            var result = Result<string>.FromException(null);
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            await Assert.That(ex.ParamName).IsEqualTo("exception");
        }
    }
    
    #endregion
    
    #region Implicit Conversion Tests
    
    [Test]
    public async Task ImplicitConversion_FromResultToValue_ShouldReturnData_WhenResultIsSuccessful()
    {
        // Arrange
        var expectedValue = "Test data";
        var result = Result<string>.Ok(expectedValue);
        
        // Act
        string value = result; // Implicit conversion
        
        // Assert
        await Assert.That(value).IsEqualTo(expectedValue);
    }
    
    [Test]
    public async Task ImplicitConversion_FromResultToValue_ShouldThrowException_WhenResultIsFailed()
    {
        // Arrange
        var errorMessage = "Operation failed";
        var result = Result<string>.Fail(errorMessage);
        
        // Act & Assert
        try
        {
            string value = result; // Implicit conversion
            // If we get here, the test should fail because no exception was thrown
            await Assert.That(false).IsTrue();
            Console.WriteLine("Expected exception was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            await Assert.That(ex.Message).IsEqualTo(errorMessage);
        }
    }
    
    [Test]
    public async Task ImplicitConversion_FromValueToResult_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "Test data";
        
        // Act
        Result<string> result = value; // Implicit conversion
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).IsEqualTo(value);
        await Assert.That(result.Message).IsEqualTo("Operation completed successfully");
    }
    
    [Test]
    public async Task ImplicitConversion_ShouldWorkWithValueTypes()
    {
        // Arrange
        int value = 42;
        
        // Act
        Result<int> result = value; // Implicit conversion from value to result
        int extractedValue = result; // Implicit conversion from result to value
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).IsEqualTo(value);
        await Assert.That(extractedValue).IsEqualTo(value);
    }
    
    [Test]
    public async Task ImplicitConversion_ShouldWorkWithReferenceTypes()
    {
        // Arrange
        var obj = new TestObject { Id = 1, Name = "Test" };
        
        // Act
        Result<TestObject> result = obj; // Implicit conversion from value to result
        TestObject extractedObj = result; // Implicit conversion from result to value
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).IsEqualTo(obj);
        await Assert.That(extractedObj).IsEqualTo(obj);
    }
    
    [Test]
    public async Task ImplicitConversion_ShouldWorkWithNullableValueTypes()
    {
        // Arrange
        int? value = 42;
        
        // Act
        Result<int?> result = value; // Implicit conversion from value to result
        int? extractedValue = result; // Implicit conversion from result to value
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).IsEqualTo(value);
        await Assert.That(extractedValue).IsEqualTo(value);
    }
    
    #endregion
    
    #region Error Aggregation Tests
    
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
    
    #endregion
    
    #region Consistent API Design Tests
    
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
        var executionOrder = new System.Text.StringBuilder();
        
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
        var executionOrder = new System.Text.StringBuilder();
        
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
        var executionOrder = new System.Text.StringBuilder();
        
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
    
    #endregion
}
}
