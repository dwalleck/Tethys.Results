using BenchmarkDotNet.Attributes;

namespace Tethys.Results.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class FunctionalMethodsBenchmarks
{
    private readonly Result<int> _successResult = Result<int>.Ok(42);
    private readonly Result<int> _failureResult = Result<int>.Fail("Operation failed");

    [Benchmark]
    public string MatchSuccess()
    {
        return _successResult.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error}"
        );
    }

    [Benchmark]
    public string MatchFailure()
    {
        return _failureResult.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error}"
        );
    }

    [Benchmark]
    public Result<int> MapSuccess()
    {
        return _successResult.Map(value => value * 2);
    }

    [Benchmark]
    public Result<int> MapFailure()
    {
        return _failureResult.Map(value => value * 2);
    }

    [Benchmark]
    public Result<int> FlatMapSuccess()
    {
        return _successResult.FlatMap(value => Result<int>.Ok(value * 2));
    }

    [Benchmark]
    public Result<int> FlatMapFailure()
    {
        return _failureResult.FlatMap(value => Result<int>.Ok(value * 2));
    }

    [Benchmark]
    public Result<int> MapErrorSuccess()
    {
        return _successResult.MapError(error => new Exception($"Mapped: {error.Message}"));
    }

    [Benchmark]
    public Result<int> MapErrorFailure()
    {
        return _failureResult.MapError(error => new Exception($"Mapped: {error.Message}"));
    }

    [Benchmark]
    public Result<int> ChainingOperations()
    {
        return _successResult
            .Map(value => value * 2)
            .FlatMap(value => Result<int>.Ok(value + 10))
            .Map(value => value / 2);
    }

    [Benchmark]
    public Result<string> ComplexTransformation()
    {
        return _successResult
            .Map(value => value.ToString())
            .FlatMap(str => Result<string>.Ok($"Value: {str}"))
            .MapError(error => new Exception($"Error occurred: {error.Message}"));
    }
}