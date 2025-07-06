using System;
using System.Threading;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class CallbackTests
    {
        #region OnSuccess Tests for Result

        [Test]
        public async Task OnSuccess_Result_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok();
            var callbackExecuted = false;

            // Act
            result.OnSuccess(() => callbackExecuted = true);

            // Assert
            await Assert.That(callbackExecuted).IsTrue();
        }

        [Test]
        public async Task OnSuccess_Result_DoesNotExecuteCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result.Fail("Error");
            var callbackExecuted = false;

            // Act
            result.OnSuccess(() => callbackExecuted = true);

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnSuccess_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok("Success message");

            // Act
            var returnedResult = result.OnSuccess(() => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Success message");
        }

        [Test]
        public async Task OnSuccess_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Ok();
            Action callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnSuccess(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        [Test]
        public async Task OnSuccess_Result_HandlesExceptionInCallback()
        {
            // Arrange
            var result = Result.Ok();
            var expectedException = new InvalidOperationException("Callback error");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
            {
                result.OnSuccess(() => throw expectedException);
                return Task.CompletedTask;
            });
        }

        #endregion

        #region OnSuccess Tests for Result<T>

        [Test]
        public async Task OnSuccess_ResultT_ExecutesCallbackWithData_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var capturedValue = 0;

            // Act
            result.OnSuccess(value => capturedValue = value);

            // Assert
            await Assert.That(capturedValue).IsEqualTo(42);
        }

        [Test]
        public async Task OnSuccess_ResultT_DoesNotExecuteCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            var callbackExecuted = false;

            // Act
            result.OnSuccess(value => callbackExecuted = true);

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnSuccess_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<string>.Ok("test", "Success message");

            // Act
            var returnedResult = result.OnSuccess(value => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Data).IsEqualTo("test");
            await Assert.That(returnedResult.Message).IsEqualTo("Success message");
        }

        [Test]
        public async Task OnSuccess_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            Action<int> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnSuccess(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        [Test]
        public async Task OnSuccess_ResultT_WorksWithReferenceTypes()
        {
            // Arrange
            var data = new DataClass();
            var result = Result<DataClass>.Ok(data);
            DataClass capturedData = null;

            // Act
            result.OnSuccess(value => capturedData = value);

            // Assert
            await Assert.That(capturedData).IsSameReferenceAs(data);
        }

        [Test]
        public async Task OnSuccess_ResultT_WorksWithNullData()
        {
            // Arrange
            string data = null;
            var result = Result<string>.Ok(data);
            var callbackExecuted = false;
            string capturedValue = "not null";

            // Act
            result.OnSuccess(value => 
            {
                callbackExecuted = true;
                capturedValue = value;
            });

            // Assert
            await Assert.That(callbackExecuted).IsTrue();
            await Assert.That(capturedValue).IsNull();
        }

        #endregion

        #region OnFailure Tests for Result

        [Test]
        public async Task OnFailure_Result_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result.Fail("Error", exception);
            Exception capturedEx = null;

            // Act
            result.OnFailure(ex => capturedEx = ex);

            // Assert
            await Assert.That(capturedEx).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task OnFailure_Result_ExecutesCallback_WhenResultIsFailedWithoutException()
        {
            // Arrange
            var result = Result.Fail("Error message");
            Exception capturedEx = new Exception("not null");

            // Act
            result.OnFailure(ex => capturedEx = ex);

            // Assert
            await Assert.That(capturedEx).IsNull();
        }

        [Test]
        public async Task OnFailure_Result_DoesNotExecuteCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok();
            var callbackExecuted = false;

            // Act
            result.OnFailure(ex => callbackExecuted = true);

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnFailure_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Fail("Error message");

            // Act
            var returnedResult = result.OnFailure(ex => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Error message");
        }

        [Test]
        public async Task OnFailure_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Fail("Error");
            Action<Exception> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnFailure(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region OnFailure Tests for Result<T>

        [Test]
        public async Task OnFailure_ResultT_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result<int>.Fail("Error", exception);
            Exception capturedEx = null;

            // Act
            result.OnFailure(ex => capturedEx = ex);

            // Assert
            await Assert.That(capturedEx).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task OnFailure_ResultT_DoesNotExecuteCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var callbackExecuted = false;

            // Act
            result.OnFailure(ex => callbackExecuted = true);

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnFailure_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<int>.Fail("Error message");

            // Act
            var returnedResult = result.OnFailure(ex => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Error message");
        }

        [Test]
        public async Task OnFailure_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            Action<Exception> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnFailure(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region OnBoth Tests for Result

        [Test]
        public async Task OnBoth_Result_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok("Success");
            Result capturedResult = null;

            // Act
            result.OnBoth(r => capturedResult = r);

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsTrue();
        }

        [Test]
        public async Task OnBoth_Result_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result.Fail("Error");
            Result capturedResult = null;

            // Act
            result.OnBoth(r => capturedResult = r);

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsFalse();
        }

        [Test]
        public async Task OnBoth_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok();

            // Act
            var returnedResult = result.OnBoth(r => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
        }

        [Test]
        public async Task OnBoth_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Ok();
            Action<Result> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnBoth(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region OnBoth Tests for Result<T>

        [Test]
        public async Task OnBoth_ResultT_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42, "Success");
            Result<int> capturedResult = null;

            // Act
            result.OnBoth(r => capturedResult = r);

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsTrue();
            await Assert.That(capturedResult.Data).IsEqualTo(42);
        }

        [Test]
        public async Task OnBoth_ResultT_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            Result<int> capturedResult = null;

            // Act
            result.OnBoth(r => capturedResult = r);

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsFalse();
        }

        [Test]
        public async Task OnBoth_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<string>.Ok("test");

            // Act
            var returnedResult = result.OnBoth(r => { });

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
        }

        [Test]
        public async Task OnBoth_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            Action<Result<int>> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            {
                result.OnBoth(callback);
                return Task.CompletedTask;
            });
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnSuccess Tests for Result

        [Test]
        public async Task OnSuccessAsync_Result_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok();
            var callbackExecuted = false;

            // Act
            await result.OnSuccessAsync(async () =>
            {
                await Task.Delay(10);
                callbackExecuted = true;
            });

            // Assert
            await Assert.That(callbackExecuted).IsTrue();
        }

        [Test]
        public async Task OnSuccessAsync_Result_DoesNotExecuteCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result.Fail("Error");
            var callbackExecuted = false;

            // Act
            await result.OnSuccessAsync(async () =>
            {
                await Task.Delay(10);
                callbackExecuted = true;
            });

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnSuccessAsync_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok("Success message");

            // Act
            var returnedResult = await result.OnSuccessAsync(async () => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Success message");
        }

        [Test]
        public async Task OnSuccessAsync_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Ok();
            Func<Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnSuccessAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnSuccess Tests for Result<T>

        [Test]
        public async Task OnSuccessAsync_ResultT_ExecutesCallbackWithData_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var capturedValue = 0;

            // Act
            await result.OnSuccessAsync(async value =>
            {
                await Task.Delay(10);
                capturedValue = value;
            });

            // Assert
            await Assert.That(capturedValue).IsEqualTo(42);
        }

        [Test]
        public async Task OnSuccessAsync_ResultT_DoesNotExecuteCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            var callbackExecuted = false;

            // Act
            await result.OnSuccessAsync(async value =>
            {
                await Task.Delay(10);
                callbackExecuted = true;
            });

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnSuccessAsync_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<string>.Ok("test", "Success message");

            // Act
            var returnedResult = await result.OnSuccessAsync(async value => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Data).IsEqualTo("test");
            await Assert.That(returnedResult.Message).IsEqualTo("Success message");
        }

        [Test]
        public async Task OnSuccessAsync_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            Func<int, Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnSuccessAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnFailure Tests for Result

        [Test]
        public async Task OnFailureAsync_Result_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result.Fail("Error", exception);
            Exception capturedEx = null;

            // Act
            await result.OnFailureAsync(async ex =>
            {
                await Task.Delay(10);
                capturedEx = ex;
            });

            // Assert
            await Assert.That(capturedEx).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task OnFailureAsync_Result_DoesNotExecuteCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok();
            var callbackExecuted = false;

            // Act
            await result.OnFailureAsync(async ex =>
            {
                await Task.Delay(10);
                callbackExecuted = true;
            });

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnFailureAsync_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Fail("Error message");

            // Act
            var returnedResult = await result.OnFailureAsync(async ex => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Error message");
        }

        [Test]
        public async Task OnFailureAsync_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Fail("Error");
            Func<Exception, Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnFailureAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnFailure Tests for Result<T>

        [Test]
        public async Task OnFailureAsync_ResultT_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");
            var result = Result<int>.Fail("Error", exception);
            Exception capturedEx = null;

            // Act
            await result.OnFailureAsync(async ex =>
            {
                await Task.Delay(10);
                capturedEx = ex;
            });

            // Assert
            await Assert.That(capturedEx).IsSameReferenceAs(exception);
        }

        [Test]
        public async Task OnFailureAsync_ResultT_DoesNotExecuteCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var callbackExecuted = false;

            // Act
            await result.OnFailureAsync(async ex =>
            {
                await Task.Delay(10);
                callbackExecuted = true;
            });

            // Assert
            await Assert.That(callbackExecuted).IsFalse();
        }

        [Test]
        public async Task OnFailureAsync_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<int>.Fail("Error message");

            // Act
            var returnedResult = await result.OnFailureAsync(async ex => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
            await Assert.That(returnedResult.Message).IsEqualTo("Error message");
        }

        [Test]
        public async Task OnFailureAsync_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            Func<Exception, Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnFailureAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnBoth Tests for Result

        [Test]
        public async Task OnBothAsync_Result_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result.Ok("Success");
            Result capturedResult = null;

            // Act
            await result.OnBothAsync(async r =>
            {
                await Task.Delay(10);
                capturedResult = r;
            });

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsTrue();
        }

        [Test]
        public async Task OnBothAsync_Result_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result.Fail("Error");
            Result capturedResult = null;

            // Act
            await result.OnBothAsync(async r =>
            {
                await Task.Delay(10);
                capturedResult = r;
            });

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsFalse();
        }

        [Test]
        public async Task OnBothAsync_Result_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok();

            // Act
            var returnedResult = await result.OnBothAsync(async r => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
        }

        [Test]
        public async Task OnBothAsync_Result_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result.Ok();
            Func<Result, Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnBothAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Async OnBoth Tests for Result<T>

        [Test]
        public async Task OnBothAsync_ResultT_ExecutesCallback_WhenResultIsSuccessful()
        {
            // Arrange
            var result = Result<int>.Ok(42, "Success");
            Result<int> capturedResult = null;

            // Act
            await result.OnBothAsync(async r =>
            {
                await Task.Delay(10);
                capturedResult = r;
            });

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsTrue();
            await Assert.That(capturedResult.Data).IsEqualTo(42);
        }

        [Test]
        public async Task OnBothAsync_ResultT_ExecutesCallback_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Error");
            Result<int> capturedResult = null;

            // Act
            await result.OnBothAsync(async r =>
            {
                await Task.Delay(10);
                capturedResult = r;
            });

            // Assert
            await Assert.That(capturedResult).IsSameReferenceAs(result);
            await Assert.That(capturedResult.Success).IsFalse();
        }

        [Test]
        public async Task OnBothAsync_ResultT_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result<string>.Ok("test");

            // Act
            var returnedResult = await result.OnBothAsync(async r => await Task.Delay(10));

            // Assert
            await Assert.That(returnedResult).IsSameReferenceAs(result);
        }

        [Test]
        public async Task OnBothAsync_ResultT_ThrowsArgumentNullException_WhenCallbackIsNull()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            Func<Result<int>, Task> callback = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.OnBothAsync(callback));
            await Assert.That(exception.ParamName).IsEqualTo("callback");
        }

        #endregion

        #region Thread Safety Tests

        [Test]
        public async Task Callbacks_AreThreadSafe()
        {
            // Arrange
            var result = Result<int>.Ok(42);
            var counter = 0;
            var tasks = new Task[100];

            // Act
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    result
                        .OnSuccess(value => Interlocked.Increment(ref counter))
                        .OnFailure(ex => Interlocked.Decrement(ref counter))
                        .OnBoth(r => Interlocked.Add(ref counter, 0));
                });
            }

            await Task.WhenAll(tasks);

            // Assert
            await Assert.That(counter).IsEqualTo(100);
        }

        #endregion
    }
}