using BenchmarkDotNet.Attributes;

namespace Tethys.Results.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class CombineBenchmarks
{
    private List<Result> _successResults = null!;
    private List<Result> _mixedResults = null!;
    private List<Result> _failureResults = null!;
    private List<Result<int>> _genericSuccessResults = null!;
    private List<Result<int>> _genericMixedResults = null!;

    [Params(2, 5, 10, 50, 100)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _successResults = new List<Result>(Count);
        _mixedResults = new List<Result>(Count);
        _failureResults = new List<Result>(Count);
        _genericSuccessResults = new List<Result<int>>(Count);
        _genericMixedResults = new List<Result<int>>(Count);

        for (int i = 0; i < Count; i++)
        {
            _successResults.Add(Result.Ok());
            _failureResults.Add(Result.Fail($"Error {i}"));
            
            // Mixed: alternating success and failure
            _mixedResults.Add(i % 2 == 0 ? Result.Ok() : Result.Fail($"Error {i}"));
            
            _genericSuccessResults.Add(Result<int>.Ok(i));
            _genericMixedResults.Add(i % 2 == 0 
                ? Result<int>.Ok(i) 
                : Result<int>.Fail($"Error {i}"));
        }
    }

    [Benchmark]
    public Result CombineAllSuccess()
    {
        return Result.Combine(_successResults);
    }

    [Benchmark]
    public Result CombineAllFailure()
    {
        return Result.Combine(_failureResults);
    }

    [Benchmark]
    public Result CombineMixed()
    {
        return Result.Combine(_mixedResults);
    }

    [Benchmark]
    public Result<IEnumerable<int>> CombineGenericAllSuccess()
    {
        return Result<int>.Combine(_genericSuccessResults);
    }

    [Benchmark]
    public Result<IEnumerable<int>> CombineGenericMixed()
    {
        return Result<int>.Combine(_genericMixedResults);
    }

    [Benchmark]
    public Result CombineWithArray()
    {
        return Result.Combine(_successResults.ToArray());
    }

    [Benchmark]
    public Result CombineWithEnumerable()
    {
        return Result.Combine(_successResults.AsEnumerable());
    }
}