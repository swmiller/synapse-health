using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Synapse.SignalBoosterExample;
using Synapse.SignalBoosterExample.Interfaces;
using Synapse.SignalBoosterExample.Services;
using Xunit;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace SignalBooster.Tests;

public class PhysicianNoteToDMEParserTests
{
    private readonly Mock<ILogger<PhysicianNoteToDMEParser>> _loggerMock;
    private readonly PhysicianNoteToDMEParser _parser;

    public PhysicianNoteToDMEParserTests()
    {
        _loggerMock = new Mock<ILogger<PhysicianNoteToDMEParser>>();
        _parser = new PhysicianNoteToDMEParser(_loggerMock.Object);
    }

    [Fact]
    public void ParsePhysicianNote_NullOrEmptyNote_ReturnsEmptyDMEObject()
    {
        // Arrange
        string emptyNote = string.Empty;

        // Act
        var result = _parser.ParsePhysicianNote(emptyNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.Unknown, result.DeviceType);
        Assert.Equal(string.Empty, result.OrderingProvider);
        Assert.Equal(CpapMaskType.None, result.MaskType);
        VerifyLogging(_loggerMock, LogLevel.Warning, "Empty or null physician note provided to parser.");
    }

    [Fact]
    public async Task ParsePhysicianNoteAsync_ValidCpapNote_ReturnsCorrectDMEObject()
    {
        // Arrange
        string cpapNote = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

        // Act
        var result = await _parser.ParsePhysicianNoteAsync(cpapNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.CPAP, result.DeviceType);
        Assert.Equal("Dr. Cameron", result.OrderingProvider);
        Assert.Equal(CpapMaskType.FullFace, result.MaskType);
        Assert.Equal(AddOnOption.Humidifier, result.AddOn);
        Assert.Equal(20.0, result.ApneaHypopneaIndex);
    }

    [Fact]
    public void ParsePhysicianNote_OxygenTankNote_ReturnsCorrectDMEObject()
    {
        // Arrange
        string oxygenNote = "Patient requires oxygen tank 5L for sleep and exertion. Ordered by Dr. Smith.";

        // Act
        var result = _parser.ParsePhysicianNote(oxygenNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.OxygenTank, result.DeviceType);
        Assert.Equal("Dr. Smith", result.OrderingProvider);
        Assert.Equal(5.0, result.OxygenTankCapacity);
        Assert.Equal(OxygenTankUsageContext.SleepAndExertion, result.OxygenUsageContext);
    }

    [Fact]
    public void ParsePhysicianNote_WheelchairNote_ReturnsCorrectDMEObject()
    {
        // Arrange
        string wheelchairNote = "Patient needs a wheelchair for daily mobility. Ordered by Dr. Wilson.";

        // Act
        var result = _parser.ParsePhysicianNote(wheelchairNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.Wheelchair, result.DeviceType);
        Assert.Equal("Dr. Wilson", result.OrderingProvider);
    }

    [Fact]
    public void ParsePhysicianNote_UnknownDeviceType_ReturnsUnknownDeviceType()
    {
        // Arrange
        string unknownNote = "Patient requires regular checkups. Ordered by Dr. House.";

        // Act
        var result = _parser.ParsePhysicianNote(unknownNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.Unknown, result.DeviceType);
        Assert.Equal("Dr. House", result.OrderingProvider);
    }

    [Fact]
    public void ParsePhysicianNote_CpapWithNasalMask_ReturnsNasalMaskType()
    {
        // Arrange
        string nasalMaskNote = "Patient needs a CPAP with nasal mask. Ordered by Dr. Wilson.";

        // Act
        var result = _parser.ParsePhysicianNote(nasalMaskNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.CPAP, result.DeviceType);
        Assert.Equal(CpapMaskType.Nasal, result.MaskType);
    }

    [Fact]
    public void ParsePhysicianNote_CpapWithNasalPillowMask_ReturnsNasalPillowMaskType()
    {
        // Arrange
        string nasalPillowNote = "Patient needs a CPAP with nasal pillow mask. Ordered by Dr. Wilson.";

        // Act
        var result = _parser.ParsePhysicianNote(nasalPillowNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.CPAP, result.DeviceType);
        Assert.Equal(CpapMaskType.NasalPillow, result.MaskType);
    }

    [Fact]
    public void ParsePhysicianNote_OxygenForSleepOnly_ReturnsSleepUsageContext()
    {
        // Arrange
        string sleepOnlyNote = "Patient requires oxygen tank 2L for sleep. Ordered by Dr. Wilson.";

        // Act
        var result = _parser.ParsePhysicianNote(sleepOnlyNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.OxygenTank, result.DeviceType);
        Assert.Equal(OxygenTankUsageContext.Sleep, result.OxygenUsageContext);
    }

    [Fact]
    public void ParsePhysicianNote_OxygenForExertionOnly_ReturnsExertionUsageContext()
    {
        // Arrange
        string exertionOnlyNote = "Patient requires oxygen tank 3L for exertion. Ordered by Dr. Wilson.";

        // Act
        var result = _parser.ParsePhysicianNote(exertionOnlyNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DeviceType.OxygenTank, result.DeviceType);
        Assert.Equal(OxygenTankUsageContext.Exertion, result.OxygenUsageContext);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        ILogger<PhysicianNoteToDMEParser>? nullLogger = null;
        Assert.Throws<ArgumentNullException>(() => new PhysicianNoteToDMEParser(nullLogger!));
    }

    [Fact]
    public void ToJson_CpapDevice_ReturnsCorrectJson()
    {
        // Arrange
        string cpapNote = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";
        var dme = _parser.ParsePhysicianNote(cpapNote);

        // Act
        string json = dme.ToJson();

        // Assert
        Assert.Contains("\"device\":\"CPAP\"", json);
        Assert.Contains("\"mask_type\":\"FullFace\"", json);
        Assert.Contains("\"add_ons\":[\"Humidifier\"]", json);
        Assert.Contains("\"qualifier\":\"AHI > 20\"", json);
        Assert.Contains("\"ordering_provider\":\"Dr. Cameron\"", json);
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
