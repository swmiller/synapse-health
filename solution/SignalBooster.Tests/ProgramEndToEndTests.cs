using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Synapse.SignalBoosterExample;
using Synapse.SignalBoosterExample.Interfaces;
using Synapse.SignalBoosterExample.Services;
using Xunit;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace SignalBooster.Tests
{
    public class ProgramEndToEndTests
    {
        private readonly Mock<ILogger<PhysicianNoteToDMEParser>> _loggerMock;
        private readonly Mock<IPhysicianNoteReader> _noteReaderMock;
        private readonly string _expectedJson;
        private readonly string _physicianNote;

        public ProgramEndToEndTests()
        {
            _loggerMock = new Mock<ILogger<PhysicianNoteToDMEParser>>();
            _noteReaderMock = new Mock<IPhysicianNoteReader>();

            // The physician note from the requirement
            _physicianNote = @"Patient Name: Harold Finch
DOB: 04/12/1952
Diagnosis: COPD
Prescription: Requires a portable oxygen tank delivering 2 L per minute.
Usage: During sleep and exertion.
Ordering Physician: Dr. Cuddy";

            // The expected JSON from the requirement
            _expectedJson = @"{
  ""device"": ""Oxygen Tank"",
  ""liters"": ""2 L"",
  ""usage"": ""sleep and exertion"",
  ""diagnosis"": ""COPD"",
  ""ordering_provider"": ""Dr. Cuddy"",
  ""patient_name"": ""Harold Finch"",
  ""dob"": ""04/12/1952""
}";
        }

        [Fact]
        public async Task ProcessPhysicianNote_WhenGivenRequirementSample_ProducesExpectedJson()
        {
            // Arrange
            string? capturedJson = null;

            // Setup note reader mock to return our specific note
            _noteReaderMock.Setup(x => x.ReadPhysicianNoteAsync())
                .ReturnsAsync(_physicianNote);

            // Setup HTTP message handler to capture the JSON payload
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                {
                    if (request.Content != null)
                    {
                        capturedJson = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Build a service provider with our mocks
            var serviceProvider = BuildServiceProvider(httpClient);

            // Act
            // Get the services we need for testing
            var noteReader = serviceProvider.GetRequiredService<IPhysicianNoteReader>();
            var dmeParser = serviceProvider.GetRequiredService<IPhysicianNoteToDMEParser>();
            var apiClient = serviceProvider.GetRequiredService<IDmeApiClient>();

            // Simulate the process flow in Program.Main
            string noteContent = await noteReader.ReadPhysicianNoteAsync();
            var dmeData = await dmeParser.ParsePhysicianNoteAsync(noteContent);
            await apiClient.TransmitDmeObjectAsync(dmeData);

            // Assert
            Assert.NotNull(capturedJson);

            // Parse both JSON strings to compare their content rather than exact formatting
            var expectedJObject = JObject.Parse(_expectedJson);
            var actualJObject = JObject.Parse(capturedJson);

            // Check key properties
            Assert.Equal(expectedJObject["device"]?.ToString(), actualJObject["device"]?.ToString());
            Assert.Equal(expectedJObject["liters"]?.ToString(), actualJObject["liters"]?.ToString());
            Assert.Equal(expectedJObject["usage"]?.ToString(), actualJObject["usage"]?.ToString());
            Assert.Equal(expectedJObject["diagnosis"]?.ToString(), actualJObject["diagnosis"]?.ToString());
            Assert.Equal(expectedJObject["ordering_provider"]?.ToString(), actualJObject["ordering_provider"]?.ToString());
            Assert.Equal(expectedJObject["patient_name"]?.ToString(), actualJObject["patient_name"]?.ToString());
            Assert.Equal(expectedJObject["dob"]?.ToString(), actualJObject["dob"]?.ToString());
        }

        [Fact]
        public async Task ExtendedParser_WhenGivenRequirementSample_ExtractsPatientDetails()
        {
            // Arrange
            // Setup note reader mock to return our specific note
            _noteReaderMock.Setup(x => x.ReadPhysicianNoteAsync())
                .ReturnsAsync(_physicianNote);

            // Build a service provider with our mocks
            var serviceProvider = BuildServiceProvider();

            // Act
            // Get the services we need for testing
            var noteReader = serviceProvider.GetRequiredService<IPhysicianNoteReader>();
            var dmeParser = serviceProvider.GetRequiredService<IPhysicianNoteToDMEParser>();

            // Simulate part of the process flow
            string noteContent = await noteReader.ReadPhysicianNoteAsync();
            var dmeData = await dmeParser.ParsePhysicianNoteAsync(noteContent);

            // Assert
            Assert.Equal(DeviceType.OxygenTank, dmeData.DeviceType);
            Assert.Equal("Dr. Cuddy", dmeData.OrderingProvider);
            Assert.Equal(2.0, dmeData.OxygenTankCapacity);
            Assert.Equal(OxygenTankUsageContext.SleepAndExertion, dmeData.OxygenUsageContext);

            // Convert to JSON and verify specific fields
            string json = dmeData.ToJson(true);

            // Check that the JSON contains the required fields
            Assert.Contains("\"device\": \"Oxygen Tank\"", json);
            Assert.Contains("\"liters\": \"2 L\"", json);
            Assert.Contains("\"usage\": \"sleep and exertion\"", json);
            Assert.Contains("\"ordering_provider\": \"Dr. Cuddy\"", json);
        }

        private ServiceProvider BuildServiceProvider(HttpClient? httpClient = null)
        {
            // Create configuration with test settings
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            // Create a ServiceCollection and register our services
            var services = new ServiceCollection();

            // Register logger factory and loggers
            services.AddLogging(builder => builder.AddDebug());

            // Register our mocked note reader
            services.AddSingleton(_noteReaderMock.Object);

            // Register the real parser
            services.AddTransient<IPhysicianNoteToDMEParser, PhysicianNoteToDMEParser>();

            // Register the HttpClient and API client
            if (httpClient != null)
            {
                services.AddSingleton(httpClient);
                services.AddTransient<IDmeApiClient>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DmeApiClient>>();
                    return new DmeApiClient("https://alert-api.com", httpClient, logger);
                });
            }
            else
            {
                services.AddHttpClient();
                services.AddTransient<IDmeApiClient>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DmeApiClient>>();
                    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                    return new DmeApiClient("https://alert-api.com", client, logger);
                });
            }

            return services.BuildServiceProvider();
        }

        private string NormalizeJson(string json)
        {
            // Parse and re-serialize to normalize formatting
            var options = new JsonSerializerOptions { WriteIndented = false };
            using var jsonDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDoc.RootElement, options);
        }
    }
}
