using System.Threading.Tasks;

namespace Synapse.SignalBoosterExample.Interfaces
{
    /// <summary>
    /// Interface for parsing physician notes to extract durable medical equipment (DME) information.
    /// </summary>
    public interface IPhysicianNoteToDMEParser
    {
        /// <summary>
        /// Parses a physician note and extracts DME information.
        /// </summary>
        /// <param name="physicianNote">The physician note text to parse.</param>
        /// <returns>A DurableMedicalEquipment object containing the extracted information.</returns>
        DurableMedicalEquipment ParsePhysicianNote(string physicianNote);

        /// <summary>
        /// Asynchronously parses a physician note and extracts DME information.
        /// </summary>
        /// <param name="physicianNote">The physician note text to parse.</param>
        /// <returns>A Task resulting in a DurableMedicalEquipment object containing the extracted information.</returns>
        Task<DurableMedicalEquipment> ParsePhysicianNoteAsync(string physicianNote);
    }
}
