using System;
using System.Threading.Tasks;
using Tethys.Results;

namespace Tethys.Test
{
    public class TypedResultTests
    {
        [Test]
        public async Task Ok_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var value = "test value";

            // Act
            var result = Result<string, string>.Ok(value);

            // Assert
            await Assert.That(result.Success).IsTrue();
            await Assert.That(result.Value).IsEqualTo(value);
        }

        [Test]
        public async Task Fail_ShouldCreateFailedResult()
        {
            // Arrange
            var error = "validation error";

            // Act
            var result = Result<string, string>.Fail(error);

            // Assert
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Error).IsEqualTo(error);
        }
    }
}
