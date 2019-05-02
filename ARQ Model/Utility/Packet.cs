using System.Collections;

namespace ARQ_Model.Utility
{
    /// <summary>
    /// Class that represents a packet.
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// Packet class constructor.
        /// </summary>
        /// <param name="packetData">Raw data packet.</param>
        /// <param name="packetUnmodifiedData">Data packet that was not passed through noise generator.</param>
        /// <param name="index">Packet index.</param>
        public Packet(BitArray packetData, BitArray packetUnmodifiedData, int index)
        {
            PacketData = packetData;
            PacketUnmodifiedData = packetUnmodifiedData;
            Index = index;
        }

        /// <summary>
        /// Holds raw packet data.
        /// </summary>
        public BitArray PacketData { get; }
        
        /// <summary>
        /// Holds unmodified packet data.
        /// </summary>
        public BitArray PacketUnmodifiedData { get; }

        /// <summary>
        /// Packet index.
        /// </summary>
        public int Index { get; }
    }
}