using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Handles communication with external APIs for DME-related data.
    /// </summary>
    public class DmeApiClient : IDmeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DmeApiClient> _logger;
        private readonly string _baseApiUrl;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance with a specific API URL.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public DmeApiClient(string baseApiUrl, ILogger<DmeApiClient> logger)
        {
            _baseApiUrl = baseApiUrl ?? throw new ArgumentNullException(nameof(baseApiUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Initializes a new instance with a specific API URL and HttpClient (for dependency injection).
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API.</param>
        /// <param name="httpClient">HttpClient instance for API communication.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public DmeApiClient(string baseApiUrl, HttpClient httpClient, ILogger<DmeApiClient> logger)
        {
            _baseApiUrl = baseApiUrl ?? throw new ArgumentNullException(nameof(baseApiUrl));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Transmits DME data to the external API synchronously.
        /// </summary>
        /// <param name="jsonPayload">The JSON payload to send.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        public int TransmitDmeData(string jsonPayload)
        {
            _logger.LogInformation("Transmitting DME data to API: {BaseUrl}/DrExtract", _baseApiUrl);

            try
            {
                // Prepare the request
                var apiUrl = $"{_baseApiUrl}/DrExtract";
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the request synchronously
                var response = _httpClient.PostAsync(apiUrl, content).GetAwaiter().GetResult();

                // Log the result
                int statusCode = (int)response.StatusCode;
                _logger.LogInformation("API response status code: {StatusCode}", statusCode);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully transmitted DME data to API");
                }
                else
                {
                    _logger.LogWarning("Failed to transmit DME data to API. Status code: {StatusCode}", statusCode);
                }

                return statusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transmitting DME data to API");
                return (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Transmits DME data to the external API asynchronously.
        /// </summary>
        /// <param name="jsonPayload">The JSON payload to send.</param>
        /// <returns>A task representing the asynchronous operation with the HTTP status code.</returns>
        public async Task<int> TransmitDmeDataAsync(string jsonPayload)
        {
            _logger.LogInformation("Transmitting DME data to API asynchronously: {BaseUrl}/DrExtract", _baseApiUrl);

            try
            {
                // Prepare the request
                var apiUrl = $"{_baseApiUrl}/DrExtract";
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the request asynchronously
                var response = await _httpClient.PostAsync(apiUrl, content);

                // Log the result
                int statusCode = (int)response.StatusCode;
                _logger.LogInformation("API response status code: {StatusCode}", statusCode);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully transmitted DME data to API");
                }
                else
                {
                    _logger.LogWarning("Failed to transmit DME data to API. Status code: {StatusCode}", statusCode);
                }

                return statusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transmitting DME data to API");
                return (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Transmits a DME object to the external API.
        /// </summary>
        /// <param name="dmeData">The DME data object to send.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        public int TransmitDmeObject(DurableMedicalEquipment dmeData)
        {
            if (dmeData == null)
            {
                throw new ArgumentNullException(nameof(dmeData));
            }

            // Convert the DME object to a JSON string (using the ToJson method)
            string jsonPayload = dmeData.ToJson();

            // Transmit the JSON payload
            return TransmitDmeData(jsonPayload);
        }

        /// <summary>
        /// Transmits a DME object to the external API asynchronously.
        /// </summary>
        /// <param name="dmeData">The DME data object to send.</param>
        /// <returns>A task representing the asynchronous operation with the HTTP status code.</returns>
        public Task<int> TransmitDmeObjectAsync(DurableMedicalEquipment dmeData)
        {
            if (dmeData == null)
            {
                throw new ArgumentNullException(nameof(dmeData));
            }

            // Convert the DME object to a JSON string (using the ToJson method)
            string jsonPayload = dmeData.ToJson();

            // Transmit the JSON payload asynchronously
            return TransmitDmeDataAsync(jsonPayload);
        }

        /// <summary>
        /// Disposes the HttpClient if owned by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the HttpClient if owned by this instance.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Only dispose the HttpClient if we created it (not if it was injected)
                    _httpClient?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
