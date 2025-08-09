// Import basic system types
using System;
// For file reading
using System.IO;
// For HTTP requests
using System.Net.Http;
// For encoding
using System.Text;
// For async support (not used here)
using System.Threading.Tasks;
// For regex matching
using System.Text.RegularExpressions;
// For JSON object construction
using Newtonsoft.Json.Linq;

namespace Synapse.SignalBoosterExample // Namespace for the SignalBooster example
{

    /// <summary>
    /// Reads a physician note, extracts DME device information, and posts structured data to an external API.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Determine physician note file path from command line args. If the file is not
            // provided, the PhysicianNoteFileReader will fallback to a default file.
            string physicianNoteFilePath = (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                ? args[0]
                : string.Empty;

            var physicianNoteFileReader = new PhysicianNoteFileReader(physicianNoteFilePath);
            var physicianNoteText = physicianNoteFileReader.ReadPhysicianNote();

            // Device type extraction from note text
            var deviceType = "Unknown";
            if (physicianNoteText.Contains("CPAP", StringComparison.OrdinalIgnoreCase)) deviceType = "CPAP";
            else if (physicianNoteText.Contains("oxygen", StringComparison.OrdinalIgnoreCase)) deviceType = "Oxygen Tank";
            else if (physicianNoteText.Contains("wheelchair", StringComparison.OrdinalIgnoreCase)) deviceType = "Wheelchair";

            // Mask type extraction for CPAP
            string cpapMaskType = deviceType == "CPAP" && physicianNoteText.Contains("full face", StringComparison.OrdinalIgnoreCase) ? "full face" : null;
            // Add-on extraction (humidifier)
            var cpapAddOnOption = physicianNoteText.Contains("humidifier", StringComparison.OrdinalIgnoreCase) ? "humidifier" : null;
            // Qualifier extraction (AHI > 20)
            var apneaHypopneaIndexQualifier = physicianNoteText.Contains("AHI > 20") ? "AHI > 20" : "";

            // Ordering provider extraction
            var providerName = "Unknown";
            int idx = physicianNoteText.IndexOf("Dr.");
            if (idx >= 0) providerName = physicianNoteText.Substring(idx).Replace("Ordered by ", "").Trim('.');

            // Oxygen tank specific fields
            string oxygenTankLiters = null; // Liters
            var oxygenTankUsageContext = (string)null; // Usage (sleep/exertion)
            if (deviceType == "Oxygen Tank")
            {
                // Extract liters value using regex
                Match oxygenLitersMatch = Regex.Match(physicianNoteText, @"(\d+(\.\d+)?) ?L", RegexOptions.IgnoreCase);
                if (oxygenLitersMatch.Success) oxygenTankLiters = oxygenLitersMatch.Groups[1].Value + " L";

                // Extract usage context (sleep, exertion, or both)
                if (physicianNoteText.Contains("sleep", StringComparison.OrdinalIgnoreCase) && physicianNoteText.Contains("exertion", StringComparison.OrdinalIgnoreCase)) oxygenTankUsageContext = "sleep and exertion";
                else if (physicianNoteText.Contains("sleep", StringComparison.OrdinalIgnoreCase)) oxygenTankUsageContext = "sleep";
                else if (physicianNoteText.Contains("exertion", StringComparison.OrdinalIgnoreCase)) oxygenTankUsageContext = "exertion";
            }

            // Build structured JSON object for extracted data
            var deviceOrderJson = new JObject
            {
                ["device"] = deviceType,
                ["mask_type"] = cpapMaskType,
                ["add_ons"] = cpapAddOnOption != null ? new JArray(cpapAddOnOption) : null,
                ["qualifier"] = apneaHypopneaIndexQualifier,
                ["ordering_provider"] = providerName
            };

            // Add oxygen tank specific fields if applicable
            if (deviceType == "Oxygen Tank")
            {
                deviceOrderJson["liters"] = oxygenTankLiters;
                deviceOrderJson["usage"] = oxygenTankUsageContext;
            }

            // Serialize JSON object to string
            var deviceOrderJsonSerialized = deviceOrderJson.ToString();

            // Send structured data to external API via HTTP POST
            using (var httpClient = new HttpClient())
            {
                var alertApiUrl = "https://alert-api.com/DrExtract";
                var alertApiRequest = new StringContent(deviceOrderJsonSerialized, Encoding.UTF8, "application/json");
                var alertApiResponse = httpClient.PostAsync(alertApiUrl, alertApiRequest).GetAwaiter().GetResult();
            }

            // Return success code
            return 0;
        }
    }
}
