using Newtonsoft.Json.Linq;
using Synapse.SignalBoosterExample;
using System;
using Xunit;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace SignalBooster.Tests;

public class DurableMedicalEquipmentTests
{
    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        // Arrange
        var dme = new DurableMedicalEquipment
        {
            DeviceType = DeviceType.CPAP,
            OrderingProvider = "Dr. Smith",
            MaskType = CpapMaskType.FullFace,
            AddOn = AddOnOption.Humidifier,
            ApneaHypopneaIndex = 15.5,
            AdditionalNotes = "Test notes",
            ProcessedTimestamp = new DateTime(2025, 8, 10, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        string json = dme.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);

        // Verify it's valid JSON by parsing it
        var parsedJson = JObject.Parse(json);
        Assert.NotNull(parsedJson);
    }

    [Fact]
    public void ToJObject_ForCPAP_ContainsExpectedProperties()
    {
        // Arrange
        var dme = new DurableMedicalEquipment
        {
            DeviceType = DeviceType.CPAP,
            OrderingProvider = "Dr. Smith",
            MaskType = CpapMaskType.FullFace,
            AddOn = AddOnOption.Humidifier,
            ApneaHypopneaIndex = 15.5,
            ProcessedTimestamp = new DateTime(2025, 8, 10, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var jObject = dme.ToJObject();

        // Assert
        Assert.NotNull(jObject["device"]);
        Assert.Equal("CPAP", jObject["device"]?.ToString());
        Assert.NotNull(jObject["ordering_provider"]);
        Assert.Equal("Dr. Smith", jObject["ordering_provider"]?.ToString());
        Assert.NotNull(jObject["mask_type"]);
        Assert.Equal("FullFace", jObject["mask_type"]?.ToString());
        Assert.NotNull(jObject["add_ons"]);

        // Check array has items before accessing elements
        var addOns = jObject["add_ons"] as JArray;
        Assert.NotNull(addOns);
        Assert.NotEmpty(addOns);
        Assert.Equal("Humidifier", addOns[0]?.ToString());

        Assert.NotNull(jObject["qualifier"]);
        Assert.Equal("AHI > 15.5", jObject["qualifier"]?.ToString());
        Assert.NotNull(jObject["processed_timestamp"]);
        Assert.Equal("2025-08-10T10:00:00.0000000Z", jObject["processed_timestamp"]?.ToString());
    }

    [Fact]
    public void ToJObject_ForOxygenTank_ContainsExpectedProperties()
    {
        // Arrange
        var dme = new DurableMedicalEquipment
        {
            DeviceType = DeviceType.OxygenTank,
            OrderingProvider = "Dr. Johnson",
            OxygenTankCapacity = 5.0,
            OxygenUsageContext = OxygenTankUsageContext.SleepAndExertion,
            ProcessedTimestamp = new DateTime(2025, 8, 10, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var jObject = dme.ToJObject();

        // Assert
        Assert.NotNull(jObject["device"]);
        Assert.Equal("OxygenTank", jObject["device"]?.ToString());
        Assert.NotNull(jObject["ordering_provider"]);
        Assert.Equal("Dr. Johnson", jObject["ordering_provider"]?.ToString());
        Assert.NotNull(jObject["liters"]);
        Assert.Equal("5 L", jObject["liters"]?.ToString());
        Assert.NotNull(jObject["usage"]);
        Assert.Equal("sleep and exertion", jObject["usage"]?.ToString());
        Assert.NotNull(jObject["processed_timestamp"]);
        Assert.Equal("2025-08-10T10:00:00.0000000Z", jObject["processed_timestamp"]?.ToString());
    }
}
