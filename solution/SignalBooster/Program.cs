using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Synapse.SignalBoosterExample.Interfaces;
using Synapse.SignalBoosterExample.Services;

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
                    services.AddTransient<IPhysicianNoteToDMEParser, PhysicianNoteToDMEParser>();

                    // Register the DmeApiClient
                    services.AddTransient<IDmeApiClient, DmeApiClient>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<DmeApiClient>>();
                        var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                        var apiUrl = context.Configuration["ApiSettings:BaseUrl"] ?? "https://alert-api.com";
                        return new DmeApiClient(apiUrl, httpClient, logger);
                    });
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
                var dmeParser = host.Services.GetRequiredService<IPhysicianNoteToDMEParser>();
                var dmeData = await dmeParser.ParsePhysicianNoteAsync(noteContent);
                logger.LogInformation("Extracted DME data for device type: {DeviceType}", dmeData.DeviceType);

                // Transmit the extracted DME data to the external API
                var apiClient = host.Services.GetRequiredService<IDmeApiClient>();
                int statusCode = await apiClient.TransmitDmeObjectAsync(dmeData);

                if (statusCode >= 200 && statusCode < 300)
                {
                    logger.LogInformation("Successfully transmitted DME data to API with status code: {StatusCode}", statusCode);
                }
                else
                {
                    logger.LogWarning("API returned non-success status code: {StatusCode}", statusCode);
                }

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