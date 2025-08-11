using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Synapse.SignalBoosterExample.Interfaces;
using Synapse.SignalBoosterExample.Services;
using Xunit;

namespace SignalBooster.Tests;

public class PhysicianNoteFileReaderTests
{
    private readonly Mock<ILogger<PhysicianNoteFileReader>> _loggerMock;
    private const string DefaultPhysicianNoteText = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

    public PhysicianNoteFileReaderTests()
    {
        _loggerMock = new Mock<ILogger<PhysicianNoteFileReader>>();
    }

    [Fact]
    public async Task ReadPhysicianNoteAsync_FileExists_ReturnsFileContents()
    {
        // Arrange
        string testContent = "Test physician note content";
        string tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, testContent);

        try
        {
            var reader = new PhysicianNoteFileReader(tempFilePath, _loggerMock.Object);

            // Act
            string result = await reader.ReadPhysicianNoteAsync();

            // Assert
            Assert.Equal(testContent, result);
            VerifyLogging(_loggerMock, LogLevel.Information, $"Successfully read physician note from file: {tempFilePath}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ReadPhysicianNoteAsync_FileDoesNotExist_ReturnsDefaultNote()
    {
        // Arrange
        string nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var reader = new PhysicianNoteFileReader(nonExistentFilePath, _loggerMock.Object);

        // Act
        string result = await reader.ReadPhysicianNoteAsync();

        // Assert
        Assert.Equal(DefaultPhysicianNoteText, result);
        VerifyLogging(_loggerMock, LogLevel.Warning, $"File not found: {nonExistentFilePath}, using default note text");
    }

    [Fact]
    public async Task ReadPhysicianNoteAsync_ExceptionOccurs_ReturnsDefaultNote()
    {
        // Arrange
        // Create a temporary file that we'll use to force an exception
        string tempFilePath = Path.GetTempFileName();

        try
        {
            // Create a FileStream that keeps the file open with exclusive access
            using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

            // Attempt to read from the file while it's locked should throw an exception
            var reader = new PhysicianNoteFileReader(tempFilePath, _loggerMock.Object);

            // Act
            string result = await reader.ReadPhysicianNoteAsync();

            // Assert
            Assert.Equal(DefaultPhysicianNoteText, result);
            VerifyLogging(_loggerMock, LogLevel.Error, $"Error reading physician note file: {tempFilePath}");
        }
        finally
        {
            // Clean up
            try
            {
                File.Delete(tempFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task ReadPhysicianNoteAsync_EmptyFilePath_UsesDefaultFilePath()
    {
        // Arrange
        var reader = new PhysicianNoteFileReader(string.Empty, _loggerMock.Object);

        // Act
        // This test only verifies that an empty path uses the default path
        await reader.ReadPhysicianNoteAsync();

        // Assert
        // We're just checking that execution completes without error
        // The log verification confirms the default path was used
        VerifyLogging(_loggerMock, LogLevel.Debug, "PhysicianNoteFileReader initialized with file path: physician_note.txt");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        ILogger<PhysicianNoteFileReader>? nullLogger = null;
        Assert.Throws<ArgumentNullException>(() => new PhysicianNoteFileReader("test.txt", nullLogger!));
    }

    #region Helper Methods

    private void VerifyLogging<T>(Mock<ILogger<T>> loggerMock, LogLevel level, string message)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));
    }

    #endregion
}
