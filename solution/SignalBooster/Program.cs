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
            // Variable to hold the physician note text
            string x;
            try
            {
                // Default file path for physician note
                var p = "physician_note.txt";
                // If file exists, read its contents
                if (File.Exists(p))
                {
                    x = File.ReadAllText(p);
                }
                else
                {
                    // Fallback to hardcoded example note
                    x = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";
                }
            }
            catch (Exception)
            {
                // On error, fallback to hardcoded example note
                x = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";
            }

            // Optional: backup read for future expansion (currently unused)
            try
            {
                var dp = "notes_alt.txt";
                // Attempt to read alternate notes file if present
                if (File.Exists(dp)) { File.ReadAllText(dp); }
            }
            catch (Exception) { }

            // Device type extraction from note text
            var d = "Unknown";
            if (x.Contains("CPAP", StringComparison.OrdinalIgnoreCase)) d = "CPAP";
            else if (x.Contains("oxygen", StringComparison.OrdinalIgnoreCase)) d = "Oxygen Tank";
            else if (x.Contains("wheelchair", StringComparison.OrdinalIgnoreCase)) d = "Wheelchair";

            // Mask type extraction for CPAP
            string m = d == "CPAP" && x.Contains("full face", StringComparison.OrdinalIgnoreCase) ? "full face" : null;
            // Add-on extraction (humidifier)
            var a = x.Contains("humidifier", StringComparison.OrdinalIgnoreCase) ? "humidifier" : null;
            // Qualifier extraction (AHI > 20)
            var q = x.Contains("AHI > 20") ? "AHI > 20" : "";

            // Ordering provider extraction
            var pr = "Unknown";
            int idx = x.IndexOf("Dr.");
            if (idx >= 0) pr = x.Substring(idx).Replace("Ordered by ", "").Trim('.');

            // Oxygen tank specific fields
            string l = null; // Liters
            var f = (string)null; // Usage (sleep/exertion)
            if (d == "Oxygen Tank")
            {
                // Extract liters value using regex
                Match lm = Regex.Match(x, @"(\d+(\.\d+)?) ?L", RegexOptions.IgnoreCase);
                if (lm.Success) l = lm.Groups[1].Value + " L";

                // Extract usage context (sleep, exertion, or both)
                if (x.Contains("sleep", StringComparison.OrdinalIgnoreCase) && x.Contains("exertion", StringComparison.OrdinalIgnoreCase)) f = "sleep and exertion";
                else if (x.Contains("sleep", StringComparison.OrdinalIgnoreCase)) f = "sleep";
                else if (x.Contains("exertion", StringComparison.OrdinalIgnoreCase)) f = "exertion";
            }

            // Build structured JSON object for extracted data
            var r = new JObject
            {
                ["device"] = d,
                ["mask_type"] = m,
                ["add_ons"] = a != null ? new JArray(a) : null,
                ["qualifier"] = q,
                ["ordering_provider"] = pr
            };

            // Add oxygen tank specific fields if applicable
            if (d == "Oxygen Tank")
            {
                r["liters"] = l;
                r["usage"] = f;
            }

            // Serialize JSON object to string
            var sj = r.ToString();

            // Send structured data to external API via HTTP POST
            using (var h = new HttpClient())
            {
                var u = "https://alert-api.com/DrExtract";
                var c = new StringContent(sj, Encoding.UTF8, "application/json");
                var resp = h.PostAsync(u, c).GetAwaiter().GetResult();
            }

            // Return success code
            return 0;
        }
    }
}
