using System;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    public class ValueExtractionTests
    {
        [Test]
        public async Task GetValueOrDefault_ShouldReturnValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act
            var value = result.GetValueOrDefault();

            // Assert
            await Assert.That(value).IsEqualTo(expectedValue);
        }

        [Test]
        public async Task GetValueOrDefault_ShouldReturnDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<string>.Fail("Operation failed");

            // Act
            var value = result.GetValueOrDefault();

            // Assert
            await Assert.That(value).IsEqualTo(default(string));
        }

        [Test]
        public async Task GetValueOrDefault_ShouldReturnCustomDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var customDefault = "Custom default";
            var result = Result<string>.Fail("Operation failed");

            // Act
            var value = result.GetValueOrDefault(customDefault);

            // Assert
            await Assert.That(value).IsEqualTo(customDefault);
        }

        [Test]
        public async Task GetValueOrDefault_ShouldWorkWithValueTypes()
        {
            // Arrange
            var expectedValue = 42;
            var result = Result<int>.Ok(expectedValue);

            // Act
            var value = result.GetValueOrDefault();

            // Assert
            await Assert.That(value).IsEqualTo(expectedValue);
        }

        [Test]
        public async Task GetValueOrDefault_ShouldReturnCustomDefaultForValueTypes_WhenResultIsFailed()
        {
            // Arrange
            var customDefault = 99;
            var result = Result<int>.Fail("Operation failed");

            // Act
            var value = result.GetValueOrDefault(customDefault);

            // Assert
            await Assert.That(value).IsEqualTo(customDefault);
        }

        [Test]
        public async Task TryGetValue_ShouldReturnTrueAndSetValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act
            bool success = result.TryGetValue(out var value);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(value).IsEqualTo(expectedValue);
        }

        [Test]
        public async Task TryGetValue_ShouldReturnFalseAndSetDefaultValue_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<string>.Fail("Operation failed");

            // Act
            bool success = result.TryGetValue(out var value);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(value).IsEqualTo(default(string));
        }

        [Test]
        public async Task TryGetValue_ShouldWorkWithValueTypes()
        {
            // Arrange
            var expectedValue = 42;
            var result = Result<int>.Ok(expectedValue);

            // Act
            bool success = result.TryGetValue(out var value);

            // Assert
            await Assert.That(success).IsTrue();
            await Assert.That(value).IsEqualTo(expectedValue);
        }

        [Test]
        public async Task TryGetValue_ShouldSetDefaultForValueTypes_WhenResultIsFailed()
        {
            // Arrange
            var result = Result<int>.Fail("Operation failed");

            // Act
            bool success = result.TryGetValue(out var value);

            // Assert
            await Assert.That(success).IsFalse();
            await Assert.That(value).IsEqualTo(default(int));
        }

        [Test]
        public async Task GetValueOrThrow_ShouldReturnValue_WhenResultIsSuccessful()
        {
            // Arrange
            var expectedValue = "Test data";
            var result = Result<string>.Ok(expectedValue);

            // Act
            var value = result.GetValueOrThrow();

            // Assert
            await Assert.That(value).IsEqualTo(expectedValue);
        }

        [Test]
        public async Task GetValueOrThrow_ShouldThrowException_WhenResultIsFailed()
        {
            // Arrange
            var errorMessage = "Operation failed";
            var result = Result<string>.Fail(errorMessage);

            // Act & Assert
            try
            {
                var value = result.GetValueOrThrow();
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
        public async Task GetValueOrThrow_ShouldThrowOriginalException_WhenResultIsFailedWithException()
        {
            // Arrange
            var originalException = new ArgumentException("Invalid argument");
            var result = Result<string>.Fail("Operation failed", originalException);

            // Act & Assert
            try
            {
                var value = result.GetValueOrThrow();
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                await Assert.That(ex).IsEqualTo(originalException);
            }
        }

        [Test]
        public async Task ValueExtractionMethods_ShouldWorkWithReferenceTypes()
        {
            // Arrange
            var expectedObject = new TestObject { Id = 1, Name = "Test" };
            var result = Result<TestObject>.Ok(expectedObject);

            // Act & Assert
            // GetValueOrDefault
            var value1 = result.GetValueOrDefault();
            await Assert.That(value1).IsEqualTo(expectedObject);

            // TryGetValue
            bool success = result.TryGetValue(out var value2);
            await Assert.That(success).IsTrue();
            await Assert.That(value2).IsEqualTo(expectedObject);

            // GetValueOrThrow
            var value3 = result.GetValueOrThrow();
            await Assert.That(value3).IsEqualTo(expectedObject);
        }

        [Test]
        public async Task ValueExtractionMethods_ShouldWorkWithNullableValueTypes()
        {
            // Arrange
            int? expectedValue = 42;
            var result = Result<int?>.Ok(expectedValue);

            // Act & Assert
            // GetValueOrDefault
            var value1 = result.GetValueOrDefault();
            await Assert.That(value1).IsEqualTo(expectedValue);

            // With custom default
            var failedResult = Result<int?>.Fail("Failed");
            var value2 = failedResult.GetValueOrDefault(99);
            await Assert.That(value2).IsEqualTo(99);

            // TryGetValue
            bool success = result.TryGetValue(out var value3);
            await Assert.That(success).IsTrue();
            await Assert.That(value3).IsEqualTo(expectedValue);

            // GetValueOrThrow
            var value4 = result.GetValueOrThrow();
            await Assert.That(value4).IsEqualTo(expectedValue);
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
