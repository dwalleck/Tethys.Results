using BenchmarkDotNet.Attributes;

namespace Tethys.Results.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class ResultCreationBenchmarks
{
    [Benchmark]
    public Result CreateSuccess()
    {
        return Result.Ok();
    }

    [Benchmark]
    public Result CreateFailure()
    {
        return Result.Fail("Operation failed");
    }

    [Benchmark]
    public Result CreateFailureWithException()
    {
        return Result.Fail("Operation failed", new InvalidOperationException("Operation failed"));
    }

    [Benchmark]
    public Result<int> CreateGenericSuccess()
    {
        return Result<int>.Ok(42);
    }

    [Benchmark]
    public Result<string> CreateGenericSuccessString()
    {
        return Result<string>.Ok("Hello, World!");
    }

    [Benchmark]
    public Result<int> CreateGenericFailure()
    {
        return Result<int>.Fail("Operation failed");
    }

    [Benchmark]
    public Result<object> CreateGenericNullSuccess()
    {
        return Result<object>.Ok(null!);
    }

    [Benchmark]
    public Result<List<int>> CreateGenericCollectionSuccess()
    {
        return Result<List<int>>.Ok(new List<int> { 1, 2, 3, 4, 5 });
    }
}