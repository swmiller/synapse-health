using System;
using System.IO;

namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Reads the contents of a physician note file.
    /// </summary>
    public class PhysicianNoteFileReader
    {
        private readonly string _filePath;
        private const string DefaultFilePath = "physician_note.txt";
        private const string DefaultPhysicianNoteText = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

        /// <summary>
        /// Initializes a new instance with the specified physician note file path.
        /// </summary>
        /// <param name="filePath">Path to the physician note file.</param>
        public PhysicianNoteFileReader(string filePath)
        {
            // If not file path is provided, use default
            _filePath = string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : filePath;
        }

        /// <summary>
        /// Reads the contents of the physician note file. If the specified file does not exist
        /// returns a default physician note.
        /// </summary>
        /// <returns>File contents as a string.</returns>
        public string ReadPhysicianNote()
        {
            string physicianNoteText;

            try
            {
                physicianNoteText = File.Exists(_filePath)
                    ? File.ReadAllText(_filePath)
                    : DefaultPhysicianNoteText;
            }
            catch (Exception ex)
            {
                // Log the exception for observability
                Console.Error.WriteLine($"Error reading physician note file: {ex.Message}");
                physicianNoteText = DefaultPhysicianNoteText;
            }

            return physicianNoteText;
        }
    }
}
