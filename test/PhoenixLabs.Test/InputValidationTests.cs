using System;
using System.Threading.Tasks;
using Tethys.Results;
using TUnit;

namespace PhoenixLabs.Test
{
    public class InputValidationTests
    {
        [Test]
        public async Task Ok_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result.Ok(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("message");
            }
        }

        [Test]
        public async Task GenericOk_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result<string>.Ok("data", null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("message");
            }
        }

        [Test]
        public async Task Fail_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result.Fail(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("message");
            }
        }

        [Test]
        public async Task GenericFail_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result<string>.Fail(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("message");
            }
        }

        [Test]
        public async Task FromException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result.FromException(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }

        [Test]
        public async Task GenericFromException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Act & Assert
            try
            {
                var result = Result<string>.FromException(null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }
        
        [Test]
        public async Task Fail_WithMessageAndException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act & Assert
            try
            {
                var result = Result.Fail(errorMessage, null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }
        
        [Test]
        public async Task GenericFail_WithMessageAndException_ShouldThrowArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            var errorMessage = "Error message";
            
            // Act & Assert
            try
            {
                var result = Result<string>.Fail(errorMessage, null);
                // If we get here, the test should fail because no exception was thrown
                await Assert.That(false).IsTrue();
                Console.WriteLine("Expected exception was not thrown");
            }
            catch (ArgumentNullException ex)
            {
                await Assert.That(ex.ParamName).IsEqualTo("exception");
            }
        }
    }
}
