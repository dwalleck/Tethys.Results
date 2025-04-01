using System;
using PhoenixLabs.Results;

namespace TestPackage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing PhoenixLabs.Results package...");
            
            // Test basic Result
            var successResult = Result.Ok("Operation completed successfully");
            var failResult = Result.Fail("Something went wrong");
            
            Console.WriteLine($"Success result: Success={successResult.Success}, Message={successResult.Message}");
            Console.WriteLine($"Fail result: Success={failResult.Success}, Message={failResult.Message}");
            
            // Test generic Result<T>
            var dataResult = Result<int>.Ok(42);
            var failDataResult = Result<string>.Fail("Failed to get data");
            
            Console.WriteLine($"Data result: Success={dataResult.Success}, Data={dataResult.Data}");
            Console.WriteLine($"Fail data result: Success={failDataResult.Success}, Message={failDataResult.Message}");
            
            // Test value extraction
            int value = dataResult.GetValueOrDefault();
            Console.WriteLine($"Extracted value: {value}");
            
            // Test implicit conversion
            try
            {
                int answer = dataResult;
                Console.WriteLine($"Implicit conversion: {answer}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Implicit conversion failed: {ex.Message}");
            }
            
            Console.WriteLine("All tests completed successfully!");
        }
    }
}
