using System.Collections;
using System.Data.HashFunction.CRC;
using System.Linq;
using ARQ_Model.Utility;

namespace ARQ_Model.Checksum
{
    /// <summary>
    /// Cyclic redundancy check error code implementation.
    /// </summary>
    public class CyclicRedundancyCheck : IChecksum
    {
        /// <summary>
        /// CRC hash generator.
        /// </summary>
        private readonly ICRC crcGenerator;

        /// <summary>
        /// Cyclic redundancy check class constructor.
        /// </summary>
        /// <param name="config">CRC config (bit size).</param>
        public CyclicRedundancyCheck(ICRCConfig config)
        {
            crcGenerator = CRCFactory.Instance.Create(config);
        }
        
        /// <inheritdoc />
        public BitArray CalculateChecksum(BitArray packet)
        {
            var byteCount = packet.Length / 8;
            var rawBytes = new byte[byteCount];
            packet.CopyTo(rawBytes, 0);
            
            var packetHash = crcGenerator.ComputeHash(rawBytes).AsBitArray();
            var newPacket = new BitArray(packet.Length + packetHash.Length);
            for (var i = 0; i < packet.Length; i++) newPacket[i] = packet[i];
            for (var i = 0; i < packetHash.Length; i++) newPacket[packet.Length + i] = packetHash[i];
            return newPacket;
        }

        /// <inheritdoc />
        public bool CheckChecksum(BitArray packet)
        {
            var packetStripped = new BitArray(packet.Length - crcGenerator.Config.HashSizeInBits);
            for (var i = 0; i < packetStripped.Length; i++) packetStripped[i] = packet[i];
            
            var byteCount = packetStripped.Length / 8;
            var rawBytes = new byte[byteCount];
            packetStripped.CopyTo(rawBytes, 0);
            
            var packetChecksum = new BitArray(crcGenerator.HashSizeInBits);
            var bitOffset = packet.Length - crcGenerator.HashSizeInBits;
            for (var i = bitOffset; i < packet.Length; i++) packetChecksum[i - bitOffset] = packet[i];
            return crcGenerator.ComputeHash(rawBytes).AsBitArray().EqualsValue(packetChecksum);
        }

        public override string ToString()
        {
            return $"cyclic redundancy check: {crcGenerator.HashSizeInBits} bits";
        }
    }
}