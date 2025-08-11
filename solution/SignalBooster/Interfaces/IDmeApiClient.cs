namespace Synapse.SignalBoosterExample.Interfaces
{
    /// <summary>
    /// Defines operations for communicating with external APIs for DME-related data.
    /// </summary>
    public interface IDmeApiClient : IDisposable
    {
        /// <summary>
        /// Transmits DME data to the external API synchronously.
        /// </summary>
        /// <param name="jsonPayload">The JSON payload to send.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        int TransmitDmeData(string jsonPayload);

        /// <summary>
        /// Transmits DME data to the external API asynchronously.
        /// </summary>
        /// <param name="jsonPayload">The JSON payload to send.</param>
        /// <returns>A task representing the asynchronous operation with the HTTP status code.</returns>
        Task<int> TransmitDmeDataAsync(string jsonPayload);

        /// <summary>
        /// Transmits a DME object to the external API.
        /// </summary>
        /// <param name="dmeData">The DME data object to send.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        int TransmitDmeObject(DurableMedicalEquipment dmeData);

        /// <summary>
        /// Transmits a DME object to the external API asynchronously.
        /// </summary>
        /// <param name="dmeData">The DME data object to send.</param>
        /// <returns>A task representing the asynchronous operation with the HTTP status code.</returns>
        Task<int> TransmitDmeObjectAsync(DurableMedicalEquipment dmeData);
    }
}
