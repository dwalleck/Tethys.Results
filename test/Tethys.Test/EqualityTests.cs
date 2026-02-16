using System;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace Tethys.Test
{
    /// <summary>
    /// Tests for IEquatable implementation on Result types.
    /// </summary>
    public class EqualityTests
    {
        /// <summary>
        /// Tests for non-generic Result equality.
        /// </summary>
        public class ResultEqualityTests
        {
            [Test]
            public async Task Equals_WithSameSuccessfulResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result.Ok("Success");
                var result2 = Result.Ok("Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentSuccessMessages_ReturnsFalse()
            {
                // Arrange
                var result1 = Result.Ok("Success 1");
                var result2 = Result.Ok("Success 2");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSameFailedResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result.Fail("Error message");
                var result2 = Result.Fail("Error message");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentFailureMessages_ReturnsFalse()
            {
                // Arrange
                var result1 = Result.Fail("Error 1");
                var result2 = Result.Fail("Error 2");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSuccessAndFailure_ReturnsFalse()
            {
                // Arrange
                var result1 = Result.Ok("Success");
                var result2 = Result.Fail("Failure");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSameException_ReturnsTrue()
            {
                // Arrange
                var exception = new InvalidOperationException("Test exception");
                var result1 = Result.Fail("Error", exception);
                var result2 = Result.Fail("Error", exception);

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentExceptions_ReturnsFalse()
            {
                // Arrange
                var exception1 = new InvalidOperationException("Test exception 1");
                var exception2 = new InvalidOperationException("Test exception 2");
                var result1 = Result.Fail("Error", exception1);
                var result2 = Result.Fail("Error", exception2);

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithNullResult_ReturnsFalse()
            {
                // Arrange
                var result = Result.Ok("Success");
                Result nullResult = null;

                // Act & Assert
                await Assert.That(result.Equals(nullResult)).IsFalse();
            }

            [Test]
            public async Task Equals_WithNonResultObject_ReturnsFalse()
            {
                // Arrange
                var result = Result.Ok("Success");
                var notAResult = "Not a result";

                // Act & Assert
                await Assert.That(result.Equals(notAResult)).IsFalse();
            }

            [Test]
            public async Task GetHashCode_WithEqualResults_ReturnsSameHashCode()
            {
                // Arrange
                var result1 = Result.Ok("Success");
                var result2 = Result.Ok("Success");

                // Act & Assert
                await Assert.That(result1.GetHashCode()).IsEqualTo(result2.GetHashCode());
            }

            [Test]
            public async Task GetHashCode_WithDifferentResults_UsuallyReturnsDifferentHashCodes()
            {
                // Arrange
                var result1 = Result.Ok("Success 1");
                var result2 = Result.Ok("Success 2");

                // Act & Assert
                // Note: Hash codes can collide, but it should be rare
                await Assert.That(result1.GetHashCode()).IsNotEqualTo(result2.GetHashCode());
            }

            [Test]
            public async Task EqualityOperator_WithEqualResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result.Ok("Success");
                var result2 = Result.Ok("Success");

                // Act & Assert
                await Assert.That(result1 == result2).IsTrue();
            }

            [Test]
            public async Task InequalityOperator_WithDifferentResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result.Ok("Success 1");
                var result2 = Result.Ok("Success 2");

                // Act & Assert
                await Assert.That(result1 != result2).IsTrue();
            }

            [Test]
            public async Task EqualityOperator_WithNullResults_HandlesCorrectly()
            {
                // Arrange
                Result result1 = null;
                Result result2 = null;
                var result3 = Result.Ok("Success");

                // Act & Assert
                await Assert.That(result1 == result2).IsTrue();
                await Assert.That(result1 == result3).IsFalse();
                await Assert.That(result3 == result1).IsFalse();
            }
        }

        /// <summary>
        /// Tests for generic Result&lt;T&gt; equality.
        /// </summary>
        public class GenericResultEqualityTests
        {
            [Test]
            public async Task Equals_WithSameSuccessfulResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success");
                var result2 = Result<int>.Ok(42, "Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentData_ReturnsFalse()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success");
                var result2 = Result<int>.Ok(43, "Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithDifferentSuccessMessages_ReturnsFalse()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success 1");
                var result2 = Result<int>.Ok(42, "Success 2");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSameFailedResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result<int>.Fail("Error message");
                var result2 = Result<int>.Fail("Error message");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentFailureMessages_ReturnsFalse()
            {
                // Arrange
                var result1 = Result<int>.Fail("Error 1");
                var result2 = Result<int>.Fail("Error 2");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSuccessAndFailure_ReturnsFalse()
            {
                // Arrange
                var result1 = Result<int>.Ok(42);
                var result2 = Result<int>.Fail("Failure");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithSameException_ReturnsTrue()
            {
                // Arrange
                var exception = new InvalidOperationException("Test exception");
                var result1 = Result<int>.Fail("Error", exception);
                var result2 = Result<int>.Fail("Error", exception);

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result2.Equals(result1)).IsTrue();
            }

            [Test]
            public async Task Equals_WithDifferentExceptions_ReturnsFalse()
            {
                // Arrange
                var exception1 = new InvalidOperationException("Test exception 1");
                var exception2 = new InvalidOperationException("Test exception 2");
                var result1 = Result<int>.Fail("Error", exception1);
                var result2 = Result<int>.Fail("Error", exception2);

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsFalse();
                await Assert.That(result2.Equals(result1)).IsFalse();
            }

            [Test]
            public async Task Equals_WithNullResult_ReturnsFalse()
            {
                // Arrange
                var result = Result<int>.Ok(42);
                Result<int> nullResult = null;

                // Act & Assert
                await Assert.That(result.Equals(nullResult)).IsFalse();
            }

            [Test]
            public async Task Equals_WithNonResultObject_ReturnsFalse()
            {
                // Arrange
                var result = Result<int>.Ok(42);
                var notAResult = "Not a result";

                // Act & Assert
                await Assert.That(result.Equals(notAResult)).IsFalse();
            }

            [Test]
            public async Task Equals_WithNullData_HandlesCorrectly()
            {
                // Arrange
                var result1 = Result<string>.Ok(null, "Success");
                var result2 = Result<string>.Ok(null, "Success");
                var result3 = Result<string>.Ok("data", "Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
                await Assert.That(result1.Equals(result3)).IsFalse();
            }

            [Test]
            public async Task GetHashCode_WithEqualResults_ReturnsSameHashCode()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success");
                var result2 = Result<int>.Ok(42, "Success");

                // Act & Assert
                await Assert.That(result1.GetHashCode()).IsEqualTo(result2.GetHashCode());
            }

            [Test]
            public async Task GetHashCode_WithDifferentResults_UsuallyReturnsDifferentHashCodes()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success");
                var result2 = Result<int>.Ok(43, "Success");

                // Act & Assert
                // Note: Hash codes can collide, but it should be rare
                await Assert.That(result1.GetHashCode()).IsNotEqualTo(result2.GetHashCode());
            }

            [Test]
            public async Task EqualityOperator_WithEqualResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result<int>.Ok(42, "Success");
                var result2 = Result<int>.Ok(42, "Success");

                // Act & Assert
                await Assert.That(result1 == result2).IsTrue();
            }

            [Test]
            public async Task InequalityOperator_WithDifferentResults_ReturnsTrue()
            {
                // Arrange
                var result1 = Result<int>.Ok(42);
                var result2 = Result<int>.Ok(43);

                // Act & Assert
                await Assert.That(result1 != result2).IsTrue();
            }

            [Test]
            public async Task EqualityOperator_WithNullResults_HandlesCorrectly()
            {
                // Arrange
                Result<int> result1 = null;
                Result<int> result2 = null;
                var result3 = Result<int>.Ok(42);

                // Act & Assert
                await Assert.That(result1 == result2).IsTrue();
                await Assert.That(result1 == result3).IsFalse();
                await Assert.That(result3 == result1).IsFalse();
            }

            [Test]
            public async Task Equals_WithReferenceTypeData_ComparesByReference()
            {
                // Arrange
                var data = new TestClass { Value = 42 };
                var result1 = Result<TestClass>.Ok(data, "Success");
                var result2 = Result<TestClass>.Ok(data, "Success");
                var result3 = Result<TestClass>.Ok(new TestClass { Value = 42 }, "Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue(); // Same reference
                await Assert.That(result1.Equals(result3)).IsFalse(); // Different reference, even with same value
            }

            [Test]
            public async Task Equals_WithValueTypeData_ComparesByValue()
            {
                // Arrange
                var result1 = Result<DateTime>.Ok(new DateTime(2024, 1, 1), "Success");
                var result2 = Result<DateTime>.Ok(new DateTime(2024, 1, 1), "Success");

                // Act & Assert
                await Assert.That(result1.Equals(result2)).IsTrue();
            }

            private class TestClass
            {
                public int Value { get; set; }
            }
        }
    }
}