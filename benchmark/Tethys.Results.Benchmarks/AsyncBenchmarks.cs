using BenchmarkDotNet.Attributes;

namespace Tethys.Results.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class AsyncBenchmarks
{
    private readonly Result _successResult = Result.Ok();
    private readonly Result _failureResult = Result.Fail("Operation failed");
    private readonly Result<int> _genericSuccessResult = Result<int>.Ok(42);
    private readonly Result<int> _genericFailureResult = Result<int>.Fail("Operation failed");

    [Benchmark]
    public async Task<Result> ThenAsyncSuccess()
    {
        return await _successResult.ThenAsync(async () =>
        {
            await Task.Delay(1);
            return Result.Ok();
        });
    }

    [Benchmark]
    public async Task<Result> ThenAsyncFailure()
    {
        return await _failureResult.ThenAsync(async () =>
        {
            await Task.Delay(1);
            return Result.Ok();
        });
    }

    [Benchmark]
    public async Task<Result<int>> ThenAsyncGenericSuccess()
    {
        return await _genericSuccessResult.ThenAsync(async value =>
        {
            await Task.Delay(1);
            return Result<int>.Ok(value * 2);
        });
    }

    [Benchmark]
    public async Task<Result<int>> MapAsyncSuccess()
    {
        return await _genericSuccessResult.MapAsync(async value =>
        {
            await Task.Delay(1);
            return value * 2;
        });
    }

    [Benchmark]
    public async Task<Result<int>> MapAsyncFailure()
    {
        return await _genericFailureResult.MapAsync(async value =>
        {
            await Task.Delay(1);
            return value * 2;
        });
    }

    [Benchmark]
    public async Task OnSuccessAsync()
    {
        await _genericSuccessResult.OnSuccessAsync(async value =>
        {
            await Task.Delay(1);
        });
    }

    [Benchmark]
    public async Task OnFailureAsync()
    {
        await _genericFailureResult.OnFailureAsync(async error =>
        {
            await Task.Delay(1);
        });
    }

    [Benchmark]
    public async Task<Result> TryAsyncSuccess()
    {
        return await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
        });
    }

    [Benchmark]
    public async Task<Result> TryAsyncException()
    {
        return await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
        });
    }
}