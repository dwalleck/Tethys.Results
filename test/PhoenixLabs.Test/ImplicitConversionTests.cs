using System;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace PhoenixLabs.Test
{
    public class ImplicitConversionTests
    {
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
        
        [Test]
        public async Task ImplicitConversion_ShouldWorkWithNull()
        {
            // Arrange
            string value = null;
            
            // Act
            Result<string> result = value; // Implicit conversion from null to result
            
            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Data).IsNull();
        }
        
        [Test]
        public async Task ImplicitConversion_ShouldWorkInComplexExpressions()
        {
            // Arrange
            var result1 = Result<int>.Ok(10);
            var result2 = Result<int>.Ok(20);
            
            // Act - Use implicit conversion in an expression
            int sum = result1 + result2; // Implicit conversion from results to values
            
            // Assert
            await Assert.That(sum).IsEqualTo(30);
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
    }
}
