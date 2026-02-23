using System;
using System.Threading.Tasks;
using Tethys.Results;

namespace Tethys.Test
{
    public class TypedResultImplicitConversionTests
    {
        // === Domain types used across tests ===

        private sealed class StatusSynced
        {
            public string TransactionId { get; }

            public StatusSynced(string transactionId)
            {
                TransactionId = transactionId;
            }

            public override bool Equals(object obj)
            {
                return obj is StatusSynced other && TransactionId == other.TransactionId;
            }

            public override int GetHashCode()
            {
                return TransactionId?.GetHashCode() ?? 0;
            }

            public override string ToString() => $"StatusSynced({TransactionId})";
        }

        private sealed class StepFailed
        {
            public string StepName { get; }
            public Exception Exception { get; }

            public StepFailed(string stepName, Exception exception = null)
            {
                StepName = stepName;
                Exception = exception;
            }

            public override bool Equals(object obj)
            {
                return obj is StepFailed other && StepName == other.StepName;
            }

            public override int GetHashCode()
            {
                return StepName?.GetHashCode() ?? 0;
            }

            public override string ToString() => $"StepFailed({StepName})";
        }

        private enum ErrorCode
        {
            None,
            NotFound,
            Timeout
        }

        // ==========================================================
        // Success<T> wrapper → Result<TValue, TError> conversions
        // ==========================================================

        [Test]
        public async Task SuccessWrapper_WithReferenceType_ShouldCreateSuccessResult()
        {
            // Arrange
            var synced = new StatusSynced("txn-123");

            // Act
            Result<StatusSynced, StepFailed> result = Result.Ok(synced);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(synced);
        }

        [Test]
        public async Task SuccessWrapper_WithValueType_ShouldCreateSuccessResult()
        {
            // Arrange & Act
            Result<int, string> result = Result.Ok(42);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(42);
        }

        [Test]
        public async Task SuccessWrapper_WithString_ShouldCreateSuccessResult()
        {
            // Arrange & Act
            Result<string, Exception> result = Result.Ok<string>("hello");

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("hello");
        }

        // ==========================================================
        // Failure<T> wrapper → Result<TValue, TError> conversions
        // ==========================================================

        [Test]
        public async Task FailureWrapper_WithReferenceType_ShouldCreateFailedResult()
        {
            // Arrange
            var failed = new StepFailed("UpdateTransactionStatus", new InvalidOperationException("timeout"));

            // Act
            Result<StatusSynced, StepFailed> result = Result.Fail(failed);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(failed);
            await Assert.That(result.Error.StepName).IsEqualTo("UpdateTransactionStatus");
        }

        [Test]
        public async Task FailureWrapper_WithString_ShouldCreateFailedResult()
        {
            // Arrange & Act
            Result<int, string> result = Result.Fail<string>("something went wrong");

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo("something went wrong");
        }

        [Test]
        public async Task FailureWrapper_WithException_ShouldCreateFailedResult()
        {
            // Arrange
            var ex = new InvalidOperationException("bad state");

            // Act
            Result<string, Exception> result = Result.Fail<Exception>(ex);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(ex);
            await Assert.That(result.Error.Message).IsEqualTo("bad state");
        }

        // ==========================================================
        // Method return scenarios (the motivating use case)
        // ==========================================================

        [Test]
        public async Task MethodReturn_Success_ShouldWork()
        {
            // Act
            var result = SimulateSuccessfulStep();

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value.TransactionId).IsEqualTo("txn-456");
        }

        [Test]
        public async Task MethodReturn_Failure_ShouldWork()
        {
            // Act
            var result = SimulateFailedStep();

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error.StepName).IsEqualTo("UpdateTransactionStatus");
        }

        private Result<StatusSynced, StepFailed> SimulateSuccessfulStep()
        {
            return Result.Ok(new StatusSynced("txn-456"));
        }

        private Result<StatusSynced, StepFailed> SimulateFailedStep()
        {
            return Result.Fail(new StepFailed("UpdateTransactionStatus", new Exception("timeout")));
        }

        // ==========================================================
        // Same-type disambiguation (the CS0457 fix)
        // ==========================================================

        [Test]
        public async Task SameType_StringString_Ok_ShouldCreateSuccess()
        {
            // This was impossible with direct implicit operators (CS0457)
            Result<string, string> result = Result.Ok<string>("value");

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("value");
        }

        [Test]
        public async Task SameType_StringString_Fail_ShouldCreateFailure()
        {
            Result<string, string> result = Result.Fail<string>("error");

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo("error");
        }

        [Test]
        public async Task SameType_IntInt_Ok_ShouldCreateSuccess()
        {
            Result<int, int> result = Result.Ok<int>(200);

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(200);
        }

        [Test]
        public async Task SameType_IntInt_Fail_ShouldCreateFailure()
        {
            Result<int, int> result = Result.Fail<int>(404);

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(404);
        }

        // ==========================================================
        // Inheritance ambiguity fix (object, string)
        // ==========================================================

        [Test]
        public async Task InheritanceAmbiguity_ObjectString_Ok_ShouldCreateSuccess()
        {
            // With direct operators, "hello" matched TError (string) via implicit Fail.
            // Wrapper pattern forces explicit intent.
            Result<object, string> result = Result.Ok<object>("hello");

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("hello");
        }

        [Test]
        public async Task InheritanceAmbiguity_ObjectString_Fail_ShouldCreateFailure()
        {
            Result<object, string> result = Result.Fail<string>("error msg");

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo("error msg");
        }

        // ==========================================================
        // Enum types
        // ==========================================================

        [Test]
        public async Task EnumError_Ok_ShouldCreateSuccess()
        {
            Result<string, ErrorCode> result = Result.Ok<string>("data");

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("data");
        }

        [Test]
        public async Task EnumError_Fail_ShouldCreateFailure()
        {
            Result<string, ErrorCode> result = Result.Fail(ErrorCode.NotFound);

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(ErrorCode.NotFound);
        }

        // ==========================================================
        // Access guards (.Value on failure, .Error on success)
        // ==========================================================

        [Test]
        public async Task AccessGuard_ValueOnFailure_ShouldThrow()
        {
            Result<int, string> result = Result.Fail<string>("oops");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                _ = result.Value;
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task AccessGuard_ErrorOnSuccess_ShouldThrow()
        {
            Result<int, string> result = Result.Ok(42);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                _ = result.Error;
                return Task.CompletedTask;
            });
        }

        // ==========================================================
        // Equality, GetHashCode, ToString on wrapper-created results
        // ==========================================================

        [Test]
        public async Task Equality_WrapperCreated_ShouldEqualExplicitlyCreated()
        {
            Result<int, string> wrapperResult = Result.Ok(42);
            var explicitResult = Result<int, string>.Ok(42);

            await Assert.That(wrapperResult).IsEqualTo(explicitResult);
        }

        [Test]
        public async Task Equality_WrapperCreatedFailure_ShouldEqualExplicitlyCreated()
        {
            Result<int, string> wrapperResult = Result.Fail<string>("error");
            var explicitResult = Result<int, string>.Fail("error");

            await Assert.That(wrapperResult).IsEqualTo(explicitResult);
        }

        [Test]
        public async Task GetHashCode_WrapperCreated_ShouldMatchExplicit()
        {
            Result<int, string> wrapperResult = Result.Ok(42);
            var explicitResult = Result<int, string>.Ok(42);

            await Assert.That(wrapperResult.GetHashCode()).IsEqualTo(explicitResult.GetHashCode());
        }

        [Test]
        public async Task ToString_WrapperCreatedSuccess_ShouldMatchFormat()
        {
            Result<int, string> result = Result.Ok(42);

            await Assert.That(result.ToString()).IsEqualTo("Success(42)");
        }

        [Test]
        public async Task ToString_WrapperCreatedFailure_ShouldMatchFormat()
        {
            Result<int, string> result = Result.Fail<string>("oops");

            await Assert.That(result.ToString()).IsEqualTo("Failure(oops)");
        }

        // ==========================================================
        // Match on wrapper-created results
        // ==========================================================

        [Test]
        public async Task Match_OnWrapperSuccess_ShouldExecuteOnSuccess()
        {
            Result<int, string> result = Result.Ok(42);

            var output = result.Match(
                onSuccess: v => $"got {v}",
                onFailure: e => $"err {e}"
            );

            await Assert.That(output).IsEqualTo("got 42");
        }

        [Test]
        public async Task Match_OnWrapperFailure_ShouldExecuteOnFailure()
        {
            Result<int, string> result = Result.Fail<string>("bad");

            var output = result.Match(
                onSuccess: v => $"got {v}",
                onFailure: e => $"err {e}"
            );

            await Assert.That(output).IsEqualTo("err bad");
        }

        // ==========================================================
        // Wrapper struct equality and ToString
        // ==========================================================

        [Test]
        public async Task SuccessStruct_Equals_SameValue_ShouldBeEqual()
        {
            var a = Result.Ok(42);
            var b = Result.Ok(42);

            await Assert.That(a.Equals(b)).IsTrue();
        }

        [Test]
        public async Task SuccessStruct_Equals_DifferentValue_ShouldNotBeEqual()
        {
            var a = Result.Ok(42);
            var b = Result.Ok(99);

            await Assert.That(a.Equals(b)).IsFalse();
        }

        [Test]
        public async Task FailureStruct_Equals_SameValue_ShouldBeEqual()
        {
            var a = Result.Fail<string>("err");
            var b = Result.Fail<string>("err");

            await Assert.That(a.Equals(b)).IsTrue();
        }

        [Test]
        public async Task FailureStruct_Equals_DifferentValue_ShouldNotBeEqual()
        {
            var a = Result.Fail<string>("err1");
            var b = Result.Fail<string>("err2");

            await Assert.That(a.Equals(b)).IsFalse();
        }

        [Test]
        public async Task SuccessStruct_ToString_ShouldShowValue()
        {
            var s = Result.Ok(42);

            await Assert.That(s.ToString()).IsEqualTo("Success(42)");
        }

        [Test]
        public async Task FailureStruct_ToString_ShouldShowError()
        {
            var f = Result.Fail<string>("oops");

            await Assert.That(f.ToString()).IsEqualTo("Failure(oops)");
        }

        [Test]
        public async Task SuccessStruct_GetHashCode_SameValue_ShouldMatch()
        {
            var a = Result.Ok(42);
            var b = Result.Ok(42);

            await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
        }

        [Test]
        public async Task FailureStruct_GetHashCode_SameValue_ShouldMatch()
        {
            var a = Result.Fail<string>("err");
            var b = Result.Fail<string>("err");

            await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
        }

        // ==========================================================
        // Overload resolution: Result.Ok("msg") → Result, not Success<string>
        // ==========================================================

        [Test]
        public async Task OverloadResolution_OkString_ShouldReturnResult_NotSuccessWrapper()
        {
            // Result.Ok("msg") should call Result.Ok(string) returning Result,
            // NOT Result.Ok<T>(T) returning Success<string>
            var result = Result.Ok("message");

            // This should be a Result, not a Success<string>
            await Assert.That(result).IsTypeOf<Result>();
            await Assert.That(result.Success).IsTrue();
        }

        [Test]
        public async Task OverloadResolution_FailString_ShouldReturnResult_NotFailureWrapper()
        {
            // Result.Fail("msg") should call Result.Fail(string) returning Result
            var result = Result.Fail("error message");

            await Assert.That(result).IsTypeOf<Result>();
            await Assert.That(result.Success).IsFalse();
        }

        [Test]
        public async Task ExplicitGeneric_OkString_ShouldReturnSuccessWrapper()
        {
            // Result.Ok<string>("msg") explicitly calls the generic, returns Success<string>
            var wrapper = Result.Ok<string>("value");

            await Assert.That(wrapper.GetType()).IsEqualTo(typeof(Success<string>));
            await Assert.That(wrapper.Value).IsEqualTo("value");
        }

        [Test]
        public async Task OverloadResolution_FailException_ShouldReturnFailureWrapper_NotResult()
        {
            // Result.Fail(ex) resolves to Fail<T>(T) with T=Exception,
            // NOT to Fail(string, Exception). Use FromException() for that.
            var ex = new InvalidOperationException("test");
            var wrapper = Result.Fail(ex);

            await Assert.That(wrapper.GetType()).IsEqualTo(typeof(Failure<InvalidOperationException>));
            await Assert.That(wrapper.Error).IsEqualTo(ex);
        }

        // ==========================================================
        // Null handling with wrappers
        // ==========================================================

        [Test]
        public async Task SuccessWrapper_NullValue_ShouldCreateSuccessWithNull()
        {
            StatusSynced nullValue = null;
            Result<StatusSynced, StepFailed> result = Result.Ok<StatusSynced>(nullValue);

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsNull();
        }

        [Test]
        public async Task FailureWrapper_NullError_ShouldThrow()
        {
            StepFailed nullError = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                Result.Fail<StepFailed>(nullError);
                return Task.CompletedTask;
            });
        }

        // ==========================================================
        // Wrapper struct == and != operators
        // ==========================================================

        [Test]
        public async Task SuccessStruct_EqualityOperator_SameValue_ShouldBeTrue()
        {
            var a = Result.Ok(42);
            var b = Result.Ok(42);

            await Assert.That(a == b).IsTrue();
            await Assert.That(a != b).IsFalse();
        }

        [Test]
        public async Task SuccessStruct_EqualityOperator_DifferentValue_ShouldBeFalse()
        {
            var a = Result.Ok(42);
            var b = Result.Ok(99);

            await Assert.That(a == b).IsFalse();
            await Assert.That(a != b).IsTrue();
        }

        [Test]
        public async Task FailureStruct_EqualityOperator_SameValue_ShouldBeTrue()
        {
            var a = Result.Fail<string>("err");
            var b = Result.Fail<string>("err");

            await Assert.That(a == b).IsTrue();
            await Assert.That(a != b).IsFalse();
        }

        [Test]
        public async Task FailureStruct_EqualityOperator_DifferentValue_ShouldBeFalse()
        {
            var a = Result.Fail<string>("err1");
            var b = Result.Fail<string>("err2");

            await Assert.That(a == b).IsFalse();
            await Assert.That(a != b).IsTrue();
        }

        // ==========================================================
        // Wrapper struct Equals(object) with boxed and wrong types
        // ==========================================================

        [Test]
        public async Task SuccessStruct_EqualsObject_BoxedSameValue_ShouldBeTrue()
        {
            var a = Result.Ok(42);
            object boxed = Result.Ok(42);

            await Assert.That(a.Equals(boxed)).IsTrue();
        }

        [Test]
        public async Task SuccessStruct_EqualsObject_WrongType_ShouldBeFalse()
        {
            var a = Result.Ok(42);

            await Assert.That(a.Equals("not a Success<int>")).IsFalse();
            await Assert.That(a.Equals(null)).IsFalse();
            await Assert.That(a.Equals(42)).IsFalse();
        }

        [Test]
        public async Task FailureStruct_EqualsObject_BoxedSameValue_ShouldBeTrue()
        {
            var a = Result.Fail<string>("err");
            object boxed = Result.Fail<string>("err");

            await Assert.That(a.Equals(boxed)).IsTrue();
        }

        [Test]
        public async Task FailureStruct_EqualsObject_WrongType_ShouldBeFalse()
        {
            var a = Result.Fail<string>("err");

            await Assert.That(a.Equals("not a Failure<string>")).IsFalse();
            await Assert.That(a.Equals(null)).IsFalse();
            await Assert.That(a.Equals(42)).IsFalse();
        }

        // ==========================================================
        // Null ToString and GetHashCode edge cases
        // ==========================================================

        [Test]
        public async Task SuccessStruct_NullValue_ToString_ShouldShowNull()
        {
            var s = Result.Ok<string>(null);

            await Assert.That(s.ToString()).IsEqualTo("Success(null)");
        }

        [Test]
        public async Task SuccessStruct_NullValue_GetHashCode_ShouldReturnZero()
        {
            var s = Result.Ok<string>(null);

            await Assert.That(s.GetHashCode()).IsEqualTo(0);
        }
    }
}
