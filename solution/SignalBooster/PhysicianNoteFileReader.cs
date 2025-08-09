using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Reads the contents of a physician note file.
    /// </summary>
    public class PhysicianNoteFileReader : IPhysicianNoteReader
    {
        private readonly string _filePath;
        private readonly ILogger<PhysicianNoteFileReader> _logger;
        private const string DefaultFilePath = "physician_note.txt";
        private const string DefaultPhysicianNoteText = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

        /// <summary>
        /// Initializes a new instance with the specified physician note file path and logger.
        /// </summary>
        /// <param name="filePath">Path to the physician note file.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public PhysicianNoteFileReader(string filePath, ILogger<PhysicianNoteFileReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // If no file path is provided, use default
            _filePath = string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : filePath;
            _logger.LogDebug("PhysicianNoteFileReader initialized with file path: {FilePath}", _filePath);
        }

        /// <summary>
        /// Reads the contents of the physician note file. If the specified file does not exist
        /// returns a default physician note.
        /// </summary>
        /// <returns>File contents as a string.</returns>
        [Obsolete("Use ReadPhysicianNoteAsync instead")]
        public string ReadPhysicianNote()
        {
            _logger.LogWarning("Deprecated method ReadPhysicianNote called. Use ReadPhysicianNoteAsync instead.");
            return ReadPhysicianNoteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads the contents of the physician note file asynchronously. If the specified file does not exist
        /// returns a default physician note.
        /// </summary>
        /// <returns>File contents as a string.</returns>
        public async Task<string> ReadPhysicianNoteAsync()
        {
            string physicianNoteText;

            try
            {
                _logger.LogInformation("Attempting to read physician note from file: {FilePath}", _filePath);

                if (File.Exists(_filePath))
                {
                    _logger.LogDebug("File exists, reading contents");
                    physicianNoteText = await File.ReadAllTextAsync(_filePath);
                    _logger.LogInformation("Successfully read physician note from file: {FilePath}", _filePath);
                }
                else
                {
                    _logger.LogWarning("File not found: {FilePath}, using default note text", _filePath);
                    physicianNoteText = DefaultPhysicianNoteText;
                }
            }
            catch (Exception ex)
            {
                // Log the exception for observability
                _logger.LogError(ex, "Error reading physician note file: {FilePath}", _filePath);
                physicianNoteText = DefaultPhysicianNoteText;
            }

            return physicianNoteText;
        }
    }
}
