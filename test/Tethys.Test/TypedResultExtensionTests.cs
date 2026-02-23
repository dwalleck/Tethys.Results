using System;
using System.Threading;
using System.Threading.Tasks;
using Tethys.Results;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for extension methods on Result&lt;TValue, TError&gt;:
    /// Then, When, Map (sync) and ThenAsync, WhenAsync, MapAsync (async).
    /// </summary>
    public class TypedResultExtensionTests
    {
        #region Then (sync)

        [Test]
        public async Task Then_Success_ShouldExecuteNext()
        {
            var result = Result<int, string>.Ok(5);

            var chained = result.Then(v => Result<int, string>.Ok(v * 2));

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo(10);
        }

        [Test]
        public async Task Then_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("bad input");
            var wasCalled = false;

            var chained = result.Then(v =>
            {
                wasCalled = true;
                return Result<int, string>.Ok(v * 2);
            });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("bad input");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task Then_NextReturnsFailure_ShouldPropagateFailure()
        {
            var result = Result<int, string>.Ok(5);

            var chained = result.Then(v => Result<int, string>.Fail("step failed"));

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("step failed");
        }

        [Test]
        public async Task Then_TypeChanging_Success_ShouldTransform()
        {
            var result = Result<int, string>.Ok(42);

            var chained = result.Then(v => Result<string, string>.Ok($"value:{v}"));

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo("value:42");
        }

        [Test]
        public async Task Then_TypeChanging_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("not found");
            var wasCalled = false;

            var chained = result.Then(v =>
            {
                wasCalled = true;
                return Result<string, string>.Ok($"value:{v}");
            });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("not found");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public void Then_NullNext_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.Throws<ArgumentNullException>(() =>
                result.Then((Func<int, Result<int, string>>)null!));
        }

        [Test]
        public async Task Then_SuccessWithNullValue_ShouldPassNullToNext()
        {
            var result = Result<string, string>.Ok(null);

            var chained = result.Then(v => Result<string, string>.Ok(v ?? "was null"));

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo("was null");
        }

        [Test]
        public void Then_NextThrows_ShouldPropagateException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.Throws<InvalidOperationException>(() =>
                result.Then(new Func<int, Result<int, string>>(
                    v => throw new InvalidOperationException("next failed"))));
        }

        #endregion

        #region When (sync)

        [Test]
        public async Task When_SuccessAndTrue_ShouldExecuteOperation()
        {
            var result = Result<int, string>.Ok(5);

            var conditioned = result.When(true, v => Result<int, string>.Ok(v + 10));

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(15);
        }

        [Test]
        public async Task When_SuccessAndFalse_ShouldReturnOriginal()
        {
            var result = Result<int, string>.Ok(5);
            var wasCalled = false;

            var conditioned = result.When(false, v =>
            {
                wasCalled = true;
                return Result<int, string>.Ok(v + 10);
            });

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(5);
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task When_Failure_ShouldReturnOriginal()
        {
            var result = Result<int, string>.Fail("error");
            var wasCalled = false;

            var conditioned = result.When(true, v =>
            {
                wasCalled = true;
                return Result<int, string>.Ok(v + 10);
            });

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task When_OperationReturnsFailure_ShouldPropagateFailure()
        {
            var result = Result<int, string>.Ok(5);

            var conditioned = result.When(true, v => Result<int, string>.Fail("condition failed"));

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("condition failed");
        }

        [Test]
        public void When_NullOperation_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.Throws<ArgumentNullException>(() =>
                result.When(true, (Func<int, Result<int, string>>)null!));
        }

        #endregion

        #region Map (sync)

        [Test]
        public async Task Map_Success_ShouldTransformValue()
        {
            var result = Result<int, string>.Ok(5);

            var mapped = result.Map(v => v.ToString());

            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Value).IsEqualTo("5");
        }

        [Test]
        public async Task Map_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("error");
            var wasCalled = false;

            var mapped = result.Map(v =>
            {
                wasCalled = true;
                return v.ToString();
            });

            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Error).IsEqualTo("error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public void Map_NullMapper_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.Throws<ArgumentNullException>(() =>
                result.Map((Func<int, string>)null!));
        }

        [Test]
        public void Map_MapperThrows_ShouldPropagateException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.Throws<InvalidOperationException>(() =>
                result.Map(new Func<int, string>(v => throw new InvalidOperationException("mapper failed"))));
        }

        [Test]
        public async Task Map_SuccessWithNullValue_ShouldPassNullToMapper()
        {
            var result = Result<string, string>.Ok(null);

            var mapped = result.Map(v => v ?? "was null");

            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Value).IsEqualTo("was null");
        }

        #endregion

        #region ThenAsync (Result source)

        [Test]
        public async Task ThenAsync_Success_ShouldExecuteNext()
        {
            var result = Result<int, string>.Ok(5);

            var chained = await result.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v * 3);
                });

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo(15);
        }

        [Test]
        public async Task ThenAsync_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("async error");
            var wasCalled = false;

            var chained = await result.ThenAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v * 3);
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("async error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task ThenAsync_TypeChanging_Success_ShouldTransform()
        {
            var result = Result<int, string>.Ok(42);

            var chained = await result.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<string, string>.Ok($"async:{v}");
                });

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo("async:42");
        }

        [Test]
        public async Task ThenAsync_TypeChanging_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("fail");
            var wasCalled = false;

            var chained = await result.ThenAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<string, string>.Ok($"async:{v}");
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("fail");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task ThenAsync_NextReturnsFailure_ShouldPropagateFailure()
        {
            var result = Result<int, string>.Ok(5);

            var chained = await result.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Fail("async step failed");
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("async step failed");
        }

        [Test]
        public void ThenAsync_NullNext_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await result.ThenAsync((Func<int, Task<Result<int, string>>>)null!));
        }

        [Test]
        public void ThenAsync_NextThrows_ShouldPropagateException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await result.ThenAsync(new Func<int, Task<Result<int, string>>>(
                    v => throw new InvalidOperationException("async next failed"))));
        }

        #endregion

        #region ThenAsync (Task source)

        [Test]
        public async Task ThenAsync_TaskSource_Success_ShouldChain()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            var chained = await resultTask.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 1);
                });

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo(6);
        }

        [Test]
        public async Task ThenAsync_TaskSource_Failure_ShouldShortCircuit()
        {
            var resultTask = Task.FromResult(Result<int, string>.Fail("task fail"));
            var wasCalled = false;

            var chained = await resultTask.ThenAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 1);
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("task fail");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task ThenAsync_TaskSource_TypeChanging_Success_ShouldTransform()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(7));

            var chained = await resultTask.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<string, string>.Ok($"task:{v}");
                });

            await Assert.That(chained.Success).IsTrue();
            await Assert.That(chained.Value).IsEqualTo("task:7");
        }

        [Test]
        public async Task ThenAsync_TaskSource_TypeChanging_Failure_ShouldShortCircuit()
        {
            var resultTask = Task.FromResult(Result<int, string>.Fail("task error"));
            var wasCalled = false;

            var chained = await resultTask.ThenAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<string, string>.Ok($"task:{v}");
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("task error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public void ThenAsync_TaskSource_NullNext_ShouldThrowArgumentNullException()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.ThenAsync((Func<int, Task<Result<int, string>>>)null!));
        }

        [Test]
        public void ThenAsync_TaskSource_NullTask_ShouldThrowArgumentNullException()
        {
            Task<Result<int, string>> resultTask = null!;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.ThenAsync(
                    async v => Result<int, string>.Ok(v)));
        }

        [Test]
        public void ThenAsync_TaskSource_FaultedTask_ShouldPropagateException()
        {
            var resultTask = Task.FromException<Result<int, string>>(
                new InvalidOperationException("upstream failure"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await resultTask.ThenAsync(
                    async v => Result<int, string>.Ok(v)));
        }

        [Test]
        public void ThenAsync_TaskSource_CanceledTask_ShouldPropagateCancellation()
        {
            var resultTask = Task.FromCanceled<Result<int, string>>(
                new CancellationToken(true));

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await resultTask.ThenAsync(
                    async v => Result<int, string>.Ok(v)));
        }

        [Test]
        public async Task ThenAsync_TaskSource_NextReturnsFailure_ShouldPropagateFailure()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            var chained = await resultTask.ThenAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Fail("task next failed");
                });

            await Assert.That(chained.Success).IsFalse();
            await Assert.That(chained.Error).IsEqualTo("task next failed");
        }

        #endregion

        #region WhenAsync (Result source)

        [Test]
        public async Task WhenAsync_SuccessAndTrue_ShouldExecuteOperation()
        {
            var result = Result<int, string>.Ok(5);

            var conditioned = await result.WhenAsync(
                true,
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(105);
        }

        [Test]
        public async Task WhenAsync_SuccessAndFalse_ShouldReturnOriginal()
        {
            var result = Result<int, string>.Ok(5);
            var wasCalled = false;

            var conditioned = await result.WhenAsync(
                false,
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(5);
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task WhenAsync_Failure_ShouldReturnOriginal()
        {
            var result = Result<int, string>.Fail("when error");
            var wasCalled = false;

            var conditioned = await result.WhenAsync(
                true,
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("when error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task WhenAsync_OperationReturnsFailure_ShouldPropagateFailure()
        {
            var result = Result<int, string>.Ok(5);

            var conditioned = await result.WhenAsync(
                true,
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Fail("async condition failed");
                });

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("async condition failed");
        }

        [Test]
        public void WhenAsync_NullOperation_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await result.WhenAsync(true, (Func<int, Task<Result<int, string>>>)null!));
        }

        #endregion

        #region WhenAsync (Task source)

        [Test]
        public async Task WhenAsync_TaskSource_SuccessAndTrue_ShouldExecuteOperation()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            var conditioned = await resultTask.WhenAsync(
                true,
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(105);
        }

        [Test]
        public async Task WhenAsync_TaskSource_SuccessAndFalse_ShouldReturnOriginal()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));
            var wasCalled = false;

            var conditioned = await resultTask.WhenAsync(
                false,
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsTrue();
            await Assert.That(conditioned.Value).IsEqualTo(5);
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task WhenAsync_TaskSource_Failure_ShouldShortCircuit()
        {
            var resultTask = Task.FromResult(Result<int, string>.Fail("task when error"));
            var wasCalled = false;

            var conditioned = await resultTask.WhenAsync(
                true,
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 100);
                });

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("task when error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public async Task WhenAsync_TaskSource_OperationReturnsFailure_ShouldPropagateFailure()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            var conditioned = await resultTask.WhenAsync(
                true,
                async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Fail("task async condition failed");
                });

            await Assert.That(conditioned.Success).IsFalse();
            await Assert.That(conditioned.Error).IsEqualTo("task async condition failed");
        }

        [Test]
        public void WhenAsync_TaskSource_NullOperation_ShouldThrowArgumentNullException()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.WhenAsync(true, (Func<int, Task<Result<int, string>>>)null!));
        }

        [Test]
        public void WhenAsync_TaskSource_NullTask_ShouldThrowArgumentNullException()
        {
            Task<Result<int, string>> resultTask = null!;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.WhenAsync(
                    true,
                    async v => Result<int, string>.Ok(v)));
        }

        [Test]
        public void WhenAsync_TaskSource_FaultedTask_ShouldPropagateException()
        {
            var resultTask = Task.FromException<Result<int, string>>(
                new InvalidOperationException("upstream when failure"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await resultTask.WhenAsync(
                    true,
                    async v => Result<int, string>.Ok(v)));
        }

        [Test]
        public void WhenAsync_TaskSource_CanceledTask_ShouldPropagateCancellation()
        {
            var resultTask = Task.FromCanceled<Result<int, string>>(
                new CancellationToken(true));

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await resultTask.WhenAsync(
                    true,
                    async v => Result<int, string>.Ok(v)));
        }

        #endregion

        #region MapAsync (Result source)

        [Test]
        public async Task MapAsync_Success_ShouldTransformValue()
        {
            var result = Result<int, string>.Ok(42);

            var mapped = await result.MapAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return v.ToString();
                });

            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Value).IsEqualTo("42");
        }

        [Test]
        public async Task MapAsync_Failure_ShouldShortCircuit()
        {
            var result = Result<int, string>.Fail("map error");
            var wasCalled = false;

            var mapped = await result.MapAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return v.ToString();
                });

            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Error).IsEqualTo("map error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public void MapAsync_NullMapper_ShouldThrowArgumentNullException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await result.MapAsync((Func<int, Task<string>>)null!));
        }

        [Test]
        public void MapAsync_MapperThrows_ShouldPropagateException()
        {
            var result = Result<int, string>.Ok(5);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await result.MapAsync(new Func<int, Task<string>>(
                    v => throw new InvalidOperationException("async mapper failed"))));
        }

        #endregion

        #region MapAsync (Task source)

        [Test]
        public async Task MapAsync_TaskSource_Success_ShouldTransformValue()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(42));

            var mapped = await resultTask.MapAsync(
                async v =>
                {
                    await Task.Delay(1);
                    return v.ToString();
                });

            await Assert.That(mapped.Success).IsTrue();
            await Assert.That(mapped.Value).IsEqualTo("42");
        }

        [Test]
        public async Task MapAsync_TaskSource_Failure_ShouldShortCircuit()
        {
            var resultTask = Task.FromResult(Result<int, string>.Fail("task map error"));
            var wasCalled = false;

            var mapped = await resultTask.MapAsync(
                async v =>
                {
                    wasCalled = true;
                    await Task.Delay(1);
                    return v.ToString();
                });

            await Assert.That(mapped.Success).IsFalse();
            await Assert.That(mapped.Error).IsEqualTo("task map error");
            await Assert.That(wasCalled).IsFalse();
        }

        [Test]
        public void MapAsync_TaskSource_NullMapper_ShouldThrowArgumentNullException()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.MapAsync((Func<int, Task<string>>)null!));
        }

        [Test]
        public void MapAsync_TaskSource_NullTask_ShouldThrowArgumentNullException()
        {
            Task<Result<int, string>> resultTask = null!;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await resultTask.MapAsync(
                    async v => v.ToString()));
        }

        [Test]
        public void MapAsync_TaskSource_FaultedTask_ShouldPropagateException()
        {
            var resultTask = Task.FromException<Result<int, string>>(
                new InvalidOperationException("upstream map failure"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await resultTask.MapAsync(
                    async v => v.ToString()));
        }

        [Test]
        public void MapAsync_TaskSource_CanceledTask_ShouldPropagateCancellation()
        {
            var resultTask = Task.FromCanceled<Result<int, string>>(
                new CancellationToken(true));

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await resultTask.MapAsync(
                    async v => v.ToString()));
        }

        [Test]
        public void MapAsync_TaskSource_MapperThrows_ShouldPropagateException()
        {
            var resultTask = Task.FromResult(Result<int, string>.Ok(5));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await resultTask.MapAsync(new Func<int, Task<string>>(
                    v => throw new InvalidOperationException("task mapper failed"))));
        }

        #endregion

        #region Pipeline integration

        [Test]
        public async Task Pipeline_ShouldChainMultipleSteps()
        {
            var result = Result<int, string>.Ok(2)
                .Then(v => Result<int, string>.Ok(v + 3))
                .Then(v => Result<int, string>.Ok(v * 10));

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(50);
        }

        [Test]
        public async Task Pipeline_ShouldStopAtFirstFailure()
        {
            var thirdStepCalled = false;

            var result = Result<int, string>.Ok(2)
                .Then(v => Result<int, string>.Ok(v + 3))
                .Then(v => Result<int, string>.Fail("step 2 failed"))
                .Then(v =>
                {
                    thirdStepCalled = true;
                    return Result<int, string>.Ok(v * 10);
                });

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo("step 2 failed");
            await Assert.That(thirdStepCalled).IsFalse();
        }

        [Test]
        public async Task Pipeline_WithTypeChanges_ShouldWork()
        {
            var result = Result<int, string>.Ok(42)
                .Then(v => Result<string, string>.Ok(v.ToString()))
                .Then(s => Result<int, string>.Ok(s.Length));

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(2);
        }

        [Test]
        public async Task Pipeline_MixedThenAndWhen_ShouldWork()
        {
            var applyBonus = true;

            var result = Result<int, string>.Ok(100)
                .Then(v => Result<int, string>.Ok(v + 50))
                .When(applyBonus, v => Result<int, string>.Ok(v * 2))
                .Then(v => Result<int, string>.Ok(v - 10));

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(290);
        }

        [Test]
        public async Task Pipeline_ThenAndMap_ShouldWork()
        {
            var result = Result<int, string>.Ok(5)
                .Then(v => Result<int, string>.Ok(v * 2))
                .Map(v => $"result={v}");

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("result=10");
        }

        [Test]
        public async Task AsyncPipeline_ShouldChainMultipleSteps()
        {
            var result = await Result<int, string>.Ok(2)
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 3);
                })
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v * 10);
                });

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(50);
        }

        [Test]
        public async Task AsyncPipeline_ShouldStopAtFirstFailure()
        {
            var lastStepCalled = false;

            var result = await Result<int, string>.Ok(2)
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Fail("async step failed");
                })
                .ThenAsync(async v =>
                {
                    lastStepCalled = true;
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v * 10);
                });

            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo("async step failed");
            await Assert.That(lastStepCalled).IsFalse();
        }

        [Test]
        public async Task AsyncPipeline_MixedSyncAndAsync_ShouldWork()
        {
            var result = await Result<int, string>.Ok(10)
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v + 5);
                })
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<string, string>.Ok($"final:{v}");
                });

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("final:15");
        }

        [Test]
        public async Task AsyncPipeline_ThenAsyncAndMapAsync_ShouldWork()
        {
            var result = await Result<int, string>.Ok(5)
                .ThenAsync(async v =>
                {
                    await Task.Delay(1);
                    return Result<int, string>.Ok(v * 2);
                })
                .MapAsync(async v =>
                {
                    await Task.Delay(1);
                    return $"result={v}";
                });

            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo("result=10");
        }

        #endregion
    }
}
