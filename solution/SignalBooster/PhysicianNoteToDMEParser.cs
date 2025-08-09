using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Parses physician notes to extract durable medical equipment (DME) information.
    /// </summary>
    public class PhysicianNoteToDMEParser
    {
        private readonly ILogger<PhysicianNoteToDMEParser> _logger;

        /// <summary>
        /// Initializes a new instance of the PhysicianNoteToDMEParser class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        public PhysicianNoteToDMEParser(ILogger<PhysicianNoteToDMEParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses a physician note and extracts DME information.
        /// </summary>
        /// <param name="physicianNote">The physician note text to parse.</param>
        /// <returns>A DurableMedicalEquipment object containing the extracted information.</returns>
        public DurableMedicalEquipment ParsePhysicianNote(string physicianNote)
        {
            if (string.IsNullOrWhiteSpace(physicianNote))
            {
                _logger.LogWarning("Empty or null physician note provided to parser.");
                return new DurableMedicalEquipment();
            }

            _logger.LogInformation("Parsing physician note of length {Length}", physicianNote.Length);

            // Create a new DME object to populate
            var dmeData = new DurableMedicalEquipment();

            // Determine the device type based on keywords in the physician note
            dmeData.DeviceType = DetermineDeviceType(physicianNote);
            _logger.LogInformation("Identified device type: {DeviceType}", dmeData.DeviceType);

            // Determine the mask type for CPAP devices
            dmeData.MaskType = DetermineCpapMaskType(physicianNote, dmeData.DeviceType);
            _logger.LogInformation("Identified mask type: {MaskType}", dmeData.MaskType);

            // Determine add-on options (like humidifier)
            dmeData.AddOn = DetermineAddOnOption(physicianNote);
            _logger.LogInformation("Identified add-on option: {AddOn}", dmeData.AddOn);

            // Extract AHI (Apnea Hypopnea Index) value
            dmeData.ApneaHypopneaIndex = ExtractApneaHypopneaIndex(physicianNote);
            _logger.LogInformation("Extracted AHI value: {AHI}", dmeData.ApneaHypopneaIndex);

            // Extract ordering provider information
            dmeData.OrderingProvider = ExtractOrderingProvider(physicianNote);
            _logger.LogInformation("Identified ordering provider: {Provider}", dmeData.OrderingProvider);

            // Extract oxygen tank information if applicable
            if (dmeData.DeviceType == DeviceType.OxygenTank)
            {
                // Extract oxygen tank capacity
                dmeData.OxygenTankCapacity = ExtractOxygenTankCapacity(physicianNote);
                _logger.LogInformation("Extracted oxygen tank capacity: {Capacity} L", dmeData.OxygenTankCapacity);

                // Determine oxygen tank usage context
                dmeData.OxygenUsageContext = DetermineOxygenTankUsageContext(physicianNote);
                _logger.LogInformation("Identified oxygen tank usage context: {Context}", dmeData.OxygenUsageContext);
            }

            // TODO: Implement additional parsing logic to extract more DME information
            // - Extracting any additional notes or qualifiers

            _logger.LogInformation("Successfully parsed physician note, identified device type: {DeviceType}", dmeData.DeviceType);

            return dmeData;
        }

        /// <summary>
        /// Asynchronously parses a physician note and extracts DME information.
        /// </summary>
        /// <param name="physicianNote">The physician note text to parse.</param>
        /// <returns>A Task resulting in a DurableMedicalEquipment object containing the extracted information.</returns>
        public async Task<DurableMedicalEquipment> ParsePhysicianNoteAsync(string physicianNote)
        {
            // For now, this is just a wrapper around the synchronous method
            // In a real implementation, this could use async APIs for advanced parsing, external services, etc.
            return await Task.FromResult(ParsePhysicianNote(physicianNote));
        }

        /// <summary>
        /// Determines the device type from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The detected device type or Unknown if no match is found.</returns>
        private DeviceType DetermineDeviceType(string physicianNoteText)
        {
            _logger.LogDebug("Determining device type from physician note");

            // Using pattern matching to determine the device type instead of multiple if statements
            return physicianNoteText.ToUpperInvariant() switch
            {
                var note when note.Contains("CPAP") => DeviceType.CPAP,
                var note when note.Contains("OXYGEN", StringComparison.OrdinalIgnoreCase) => DeviceType.OxygenTank,
                var note when note.Contains("WHEELCHAIR", StringComparison.OrdinalIgnoreCase) => DeviceType.Wheelchair,
                _ => DeviceType.Unknown
            };
        }

        /// <summary>
        /// Determines the CPAP mask type from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <param name="deviceType">The previously determined device type.</param>
        /// <returns>The detected mask type or None if not applicable or no match is found.</returns>
        private CpapMaskType DetermineCpapMaskType(string physicianNoteText, DeviceType deviceType)
        {
            _logger.LogDebug("Determining CPAP mask type from physician note");

            // Using modern C# features with pattern matching, null-coalescing and conditional logic
            return (deviceType, physicianNoteText) switch
            {
                (DeviceType.CPAP, var note) when note.Contains("full face", StringComparison.OrdinalIgnoreCase) => CpapMaskType.FullFace,
                (DeviceType.CPAP, var note) when note.Contains("nasal pillow", StringComparison.OrdinalIgnoreCase) => CpapMaskType.NasalPillow,
                (DeviceType.CPAP, var note) when note.Contains("nasal", StringComparison.OrdinalIgnoreCase) => CpapMaskType.Nasal,
                _ => CpapMaskType.None
            };
        }

        /// <summary>
        /// Determines add-on options from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The detected add-on option or None if no match is found.</returns>
        private AddOnOption DetermineAddOnOption(string physicianNoteText)
        {
            _logger.LogDebug("Determining add-on options from physician note");

            // Using modern C# features with expression-bodied member and pattern matching
            return physicianNoteText.Contains("humidifier", StringComparison.OrdinalIgnoreCase)
                ? AddOnOption.Humidifier
                : AddOnOption.None;
        }

        /// <summary>
        /// Extracts the Apnea Hypopnea Index (AHI) value from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The extracted AHI value or null if not found.</returns>
        private double? ExtractApneaHypopneaIndex(string physicianNoteText)
        {
            _logger.LogDebug("Extracting AHI value from physician note");

            // First, check if "AHI > 20" is present (simple case)
            if (physicianNoteText.Contains("AHI > 20", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Found AHI > 20 qualifier");
                return 20.0; // Return the threshold value
            }

            // For more advanced parsing, we could use regex to extract numerical values
            // For example: var match = Regex.Match(physicianNoteText, @"AHI\s*[>≥]?\s*(\d+\.?\d*)");

            // Return null if no AHI value is found
            return null;
        }

        /// <summary>
        /// Extracts the ordering provider's name from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The extracted provider name or "Unknown" if not found.</returns>
        private string ExtractOrderingProvider(string physicianNoteText)
        {
            _logger.LogDebug("Extracting ordering provider from physician note");

            // Using modern C# features: string interpolation, null conditional operator, and pattern matching
            return physicianNoteText.IndexOf("Dr.", StringComparison.OrdinalIgnoreCase) switch
            {
                -1 => "Unknown",
                var idx => physicianNoteText[idx..]
                    .Replace("Ordered by ", "", StringComparison.OrdinalIgnoreCase)
                    .TrimEnd('.')
                    .Trim()
            };
        }

        /// <summary>
        /// Extracts the oxygen tank capacity in liters from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The extracted capacity in liters or null if not found.</returns>
        private double? ExtractOxygenTankCapacity(string physicianNoteText)
        {
            _logger.LogDebug("Extracting oxygen tank capacity from physician note");

            // Use regex to extract the numeric value followed by "L" (case-insensitive)
            var match = Regex.Match(physicianNoteText, @"(\d+(\.\d+)?) ?L", RegexOptions.IgnoreCase);

            if (match.Success && double.TryParse(match.Groups[1].Value, out double capacity))
            {
                _logger.LogInformation("Found oxygen tank capacity: {Capacity} L", capacity);
                return capacity;
            }

            _logger.LogInformation("No oxygen tank capacity found in note");
            return null;
        }

        /// <summary>
        /// Determines the oxygen tank usage context from the text of a physician note.
        /// </summary>
        /// <param name="physicianNoteText">The text of the physician note to analyze.</param>
        /// <returns>The determined usage context or None if not found.</returns>
        private OxygenTankUsageContext DetermineOxygenTankUsageContext(string physicianNoteText)
        {
            _logger.LogDebug("Determining oxygen tank usage context from physician note");

            // Check for keywords using pattern matching with a tuple of booleans
            bool containsSleep = physicianNoteText.Contains("sleep", StringComparison.OrdinalIgnoreCase);
            bool containsExertion = physicianNoteText.Contains("exertion", StringComparison.OrdinalIgnoreCase);

            return (containsSleep, containsExertion) switch
            {
                (true, true) => OxygenTankUsageContext.SleepAndExertion,
                (true, false) => OxygenTankUsageContext.Sleep,
                (false, true) => OxygenTankUsageContext.Exertion,
                _ => OxygenTankUsageContext.None
            };
        }
    }
}
