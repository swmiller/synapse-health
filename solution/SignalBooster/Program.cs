// // Import basic system types
// using System;
// // For file reading
// using System.IO;
// // For HTTP requests
// using System.Net.Http;
// // For encoding
// using System.Text;
// // For async support (not used here)
// using System.Threading.Tasks;
// // For regex matching
// using System.Text.RegularExpressions;
// // For JSON object construction
// using Newtonsoft.Json.Linq;

// namespace Synapse.SignalBoosterExample // Namespace for the SignalBooster example
// {

//     /// <summary>
//     /// Reads a physician note, extracts DME device information, and posts structured data to an external API.
//     /// </summary>
//     class Program
//     {
//         static int Main(string[] args)
//         {
//             return 0;
//         }
//     }
// }
// Import basic system types
using System;
// For file reading
using System.IO;
// For HTTP requests
using System.Net.Http;
// For encoding
using System.Text;
// For async support
using System.Threading.Tasks;
// For regex matching
using System.Text.RegularExpressions;
// For JSON object construction
using Newtonsoft.Json.Linq;
// For dependency injection and logging
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
// For HttpClient factory
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Synapse.SignalBoosterExample // Namespace for the SignalBooster example
{
    /// <summary>
    /// Reads a physician note, extracts DME device information, and posts structured data to an external API.
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Setup the dependency injection container
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register services here
                    services.AddHttpClient();

                    // Register the PhysicianNoteFileReader and its interface
                    services.AddTransient<IPhysicianNoteReader, PhysicianNoteFileReader>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<PhysicianNoteFileReader>>();
                        var filePath = context.Configuration["PhysicianNote:FilePath"] ?? "physician_note.txt";
                        return new PhysicianNoteFileReader(filePath, logger);
                    });

                    // Register the PhysicianNoteToDMEParser
                    services.AddTransient<PhysicianNoteToDMEParser>();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    // Clear default providers if needed
                    // logging.ClearProviders();

                    // Add console and debug providers
                    logging.AddConsole();
                    logging.AddDebug();

                    // Configure from appsettings.json
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                })
                .Build();

            // Get logger from the service provider
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Log the start of the application
                logger.LogInformation("Signal Booster application starting");

                // Get the IPhysicianNoteReader service from the DI container
                var noteReader = host.Services.GetRequiredService<IPhysicianNoteReader>();

                // Read the physician note
                string noteContent = await noteReader.ReadPhysicianNoteAsync();
                logger.LogInformation("Physician note content: {NoteLength} characters", noteContent.Length);

                // Parse the physician note to extract DME information
                var dmeParser = host.Services.GetRequiredService<PhysicianNoteToDMEParser>();
                var dmeData = await dmeParser.ParsePhysicianNoteAsync(noteContent);
                logger.LogInformation("Extracted DME data for device type: {DeviceType}", dmeData.DeviceType);

                // More application logic would go here

                logger.LogInformation("Signal Booster application completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                logger.LogError(ex, "An unhandled exception occurred");
                return 1;
            }
        }
    }
}