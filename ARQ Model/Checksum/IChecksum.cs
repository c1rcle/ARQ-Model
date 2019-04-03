using System.Collections;

namespace ARQ_Model.Checksum
{
    /// <summary>
    /// Interface that describes a Checksum.
    /// </summary>
    public interface IChecksum
    {
        /// <summary>
        /// Calculates checksum for a packet and appends it.
        /// </summary>
        /// <param name="packet">Packet for which we generate a checksum.</param>
        /// <returns>Packet with checksum data appended.</returns>
        BitArray CalculateChecksum(BitArray packet);

        /// <summary>
        /// Checks whether a checksum is correct.
        /// </summary>
        /// <param name="packet">Packet for which we perform a check.</param>
        /// <returns>true - packet correct, false - packet corrupted.</returns>
        bool CheckChecksum(BitArray packet);
    }
}