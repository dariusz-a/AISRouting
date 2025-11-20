using AISRouting.Infrastructure.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class PathValidatorTests
    {
        private ILogger<PathValidator> _mockLogger = null!;
        private PathValidator _validator = null!;
        private string _testFolder = null!;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<PathValidator>>();
            _validator = new PathValidator(_mockLogger);

            // Create temporary test folder
            _testFolder = Path.Combine(Path.GetTempPath(), $"AISRoutingPathValidatorTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testFolder))
            {
                Directory.Delete(_testFolder, true);
            }
        }

        [Test]
        public void ValidateOutputFilePath_WithNullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _validator.ValidateOutputFilePath(null!);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Output path cannot be empty*");
        }

        [Test]
        public void ValidateOutputFilePath_WithEmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _validator.ValidateOutputFilePath(string.Empty);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Output path cannot be empty*");
        }

        [Test]
        public void ValidateOutputFilePath_WithWhitespacePath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _validator.ValidateOutputFilePath("   ");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Output path cannot be empty*");
        }

        [Test]
        public void ValidateOutputFilePath_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testFolder, "nonexistent", "subfolder");

            // Act & Assert
            var act = () => _validator.ValidateOutputFilePath(nonExistentPath);
            act.Should().Throw<DirectoryNotFoundException>()
                .WithMessage("*Output directory not found*");
        }

        [Test]
        public void ValidateOutputFilePath_WithValidWritablePath_DoesNotThrow()
        {
            // Act & Assert
            var act = () => _validator.ValidateOutputFilePath(_testFolder);
            act.Should().NotThrow();
        }

        [Test]
        public void ValidateOutputFilePath_CreatesAndDeletesTestFile()
        {
            // Act
            _validator.ValidateOutputFilePath(_testFolder);

            // Assert - test file should be cleaned up
            var testFiles = Directory.GetFiles(_testFolder, "_write_test_*.tmp");
            testFiles.Should().BeEmpty();
        }

        [Test]
        public void ValidateInputFolderPath_WithNullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => _validator.ValidateInputFolderPath(null!);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Path cannot be null or empty*");
        }

        [Test]
        public void ValidateInputFolderPath_WithValidPath_DoesNotThrow()
        {
            // Act & Assert
            var act = () => _validator.ValidateInputFolderPath(_testFolder);
            act.Should().NotThrow();
        }
    }
}
