using BenchmarkDotNet.Attributes;

namespace Tethys.Results.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class ExceptionVsResultBenchmarks
{
    [Params(0, 10, 50, 100)]
    public int ErrorRate { get; set; }

    private readonly Random _random = new(42);

    [Benchmark]
    public int ExceptionBasedApproach()
    {
        try
        {
            return PerformOperationWithException();
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    [Benchmark]
    public int ResultBasedApproach()
    {
        var result = PerformOperationWithResult();
        return result.Success ? result.Data : -1;
    }

    [Benchmark]
    public int NestedExceptionApproach()
    {
        try
        {
            var value1 = PerformOperationWithException();
            try
            {
                var value2 = PerformOperationWithException();
                return value1 + value2;
            }
            catch (InvalidOperationException)
            {
                return -1;
            }
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    [Benchmark]
    public int NestedResultApproach()
    {
        var result1 = PerformOperationWithResult();
        if (!result1.Success) return -1;

        var result2 = PerformOperationWithResult();
        if (!result2.Success) return -1;

        return result1.Data + result2.Data;
    }

    private int PerformOperationWithException()
    {
        if (_random.Next(100) < ErrorRate)
        {
            throw new InvalidOperationException("Operation failed");
        }
        return 42;
    }

    private Result<int> PerformOperationWithResult()
    {
        if (_random.Next(100) < ErrorRate)
        {
            return Result<int>.Fail("Operation failed");
        }
        return Result<int>.Ok(42);
    }
}