using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Represents durable medical equipment (DME) data extracted from a physician note.
    /// </summary>
    public class DurableMedicalEquipment
    {
        /// <summary>
        /// The type of DME device extracted from the physician note.
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// The name of the ordering provider (physician).
        /// </summary>
        public string OrderingProvider { get; set; }

        /// <summary>
        /// The mask type for CPAP devices.
        /// </summary>
        public CpapMaskType MaskType { get; set; }

        /// <summary>
        /// Additional options for the device.
        /// </summary>
        public AddOnOption AddOn { get; set; }

        /// <summary>
        /// The usage context for oxygen tanks.
        /// </summary>
        public OxygenTankUsageContext OxygenUsageContext { get; set; }

        /// <summary>
        /// The capacity of the oxygen tank in liters, if applicable.
        /// </summary>
        public double? OxygenTankCapacity { get; set; }

        /// <summary>
        /// Apnea-Hypopnea Index value, if provided.
        /// </summary>
        public double? ApneaHypopneaIndex { get; set; }

        /// <summary>
        /// Additional notes or qualifiers extracted from the physician note.
        /// </summary>
        public string AdditionalNotes { get; set; }

        /// <summary>
        /// The patient's name from the physician note.
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// The patient's date of birth from the physician note.
        /// </summary>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// The diagnosis from the physician note.
        /// </summary>
        public string Diagnosis { get; set; }

        /// <summary>
        /// The timestamp when this DME data was extracted.
        /// </summary>
        public DateTime ProcessedTimestamp { get; set; }

        /// <summary>
        /// Constructor for the DurableMedicalEquipment class.
        /// </summary>
        public DurableMedicalEquipment()
        {
            // Default initialization
            DeviceType = DeviceType.Unknown;
            OrderingProvider = string.Empty;
            MaskType = CpapMaskType.None;
            AddOn = AddOnOption.None;
            OxygenUsageContext = OxygenTankUsageContext.None;
            AdditionalNotes = string.Empty;
            ProcessedTimestamp = DateTime.UtcNow;

            // Initialize the new properties
            PatientName = string.Empty;
            DateOfBirth = string.Empty;
            Diagnosis = string.Empty;
        }

        /// <summary>
        /// Converts the DurableMedicalEquipment object to a JSON string.
        /// </summary>
        /// <param name="formatted">Whether to format the JSON with indentation and line breaks. Default is false for compact JSON.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public string ToJson(bool formatted = false)
        {
            return ToJObject().ToString(formatted ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Converts the DurableMedicalEquipment object to a JObject for flexible JSON manipulation.
        /// </summary>
        /// <returns>A JObject representation of the object.</returns>
        public JObject ToJObject()
        {
            var jObject = new JObject
            {
                // Common properties for all device types
                ["ordering_provider"] = OrderingProvider
            };

            // Add the device type with the correct formatting
            switch (DeviceType)
            {
                case DeviceType.CPAP:
                    jObject["device"] = "CPAP";
                    break;
                case DeviceType.OxygenTank:
                    jObject["device"] = "Oxygen Tank";
                    break;
                case DeviceType.Wheelchair:
                    jObject["device"] = "Wheelchair";
                    break;
                default:
                    jObject["device"] = DeviceType.ToString();
                    break;
            }

            // Add device-specific properties based on the device type
            switch (DeviceType)
            {
                case DeviceType.CPAP:
                    // Add CPAP-specific properties
                    jObject["mask_type"] = MaskType.ToString();

                    // Add add-ons as an array if present
                    if (AddOn != AddOnOption.None)
                    {
                        jObject["add_ons"] = new JArray(AddOn.ToString());
                    }
                    else
                    {
                        jObject["add_ons"] = null;
                    }

                    // Add AHI qualifier if present
                    if (ApneaHypopneaIndex.HasValue)
                    {
                        jObject["qualifier"] = $"AHI > {ApneaHypopneaIndex.Value}";
                    }
                    else
                    {
                        jObject["qualifier"] = string.Empty;
                    }
                    break;

                case DeviceType.OxygenTank:
                    // Add oxygen tank specific properties
                    if (OxygenTankCapacity.HasValue)
                    {
                        jObject["liters"] = $"{OxygenTankCapacity.Value} L";
                    }
                    else
                    {
                        jObject["liters"] = null;
                    }

                    // Add usage context if present
                    if (OxygenUsageContext != OxygenTankUsageContext.None)
                    {
                        // Convert enum values to match original format (lowercase with spaces)
                        string usageContext = OxygenUsageContext.ToString();
                        switch (OxygenUsageContext)
                        {
                            case OxygenTankUsageContext.Sleep:
                                usageContext = "sleep";
                                break;
                            case OxygenTankUsageContext.Exertion:
                                usageContext = "exertion";
                                break;
                            case OxygenTankUsageContext.SleepAndExertion:
                                usageContext = "sleep and exertion";
                                break;
                        }
                        jObject["usage"] = usageContext;
                    }
                    else
                    {
                        jObject["usage"] = null;
                    }
                    break;

                case DeviceType.Wheelchair:
                    // Wheelchair-specific properties could be added here in the future
                    break;
            }

            // Add additional properties that weren't in the original but are useful
            jObject["processed_timestamp"] = ProcessedTimestamp.ToString("o"); // ISO 8601 format

            // Add patient information if available
            if (!string.IsNullOrEmpty(PatientName))
            {
                jObject["patient_name"] = PatientName;
            }

            if (!string.IsNullOrEmpty(DateOfBirth))
            {
                jObject["dob"] = DateOfBirth;
            }

            if (!string.IsNullOrEmpty(Diagnosis))
            {
                jObject["diagnosis"] = Diagnosis;
            }

            if (!string.IsNullOrEmpty(AdditionalNotes))
            {
                jObject["notes"] = AdditionalNotes;
            }

            return jObject;
        }
    }
}
