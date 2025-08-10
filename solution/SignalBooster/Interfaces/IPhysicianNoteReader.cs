using System.Threading.Tasks;

namespace Synapse.SignalBoosterExample.Interfaces
{
    /// <summary>
    /// Defines methods for reading physician note files.
    /// </summary>
    public interface IPhysicianNoteReader
    {
        /// <summary>
        /// Reads the contents of the physician note file asynchronously.
        /// </summary>
        /// <returns>File contents as a string.</returns>
        Task<string> ReadPhysicianNoteAsync();
    }
}
