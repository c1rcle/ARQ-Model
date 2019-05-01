using System;
using System.Collections;
using System.Linq;
using ARQ_Model.Utility;

namespace ARQ_Model.Checksum
{
    /// <summary>
    /// Bit '1' sum error code implementation.
    /// </summary>
    public class BitSum : IChecksum
    {
        /// <summary>
        /// Bit count for error code.
        /// </summary>
        private readonly int bitCount;
        
        public BitSum(int packetSize)
        {
            bitCount = (int) Math.Log(packetSize * 8, 2) + 1;
        }
        
        /// <inheritdoc />
        public BitArray CalculateChecksum(BitArray packet)
        {
            var counter = 0;
            foreach (bool item in packet) if (item) counter++;
            var checksumString = Convert.ToString(counter, 2);
            checksumString = checksumString.PadLeft(bitCount, '0');
            var checksum = new BitArray(checksumString.Select(x => x == '1').ToArray());
            
            var newPacket = new BitArray(packet.Length + bitCount);
            for (var i = 0; i < packet.Length; i++) newPacket[i] = packet[i];
            for (var i = 0; i < checksum.Length; i++) newPacket[packet.Length + i] = checksum[i];
            return newPacket;
        }

        /// <inheritdoc />
        public bool CheckChecksum(BitArray packet)
        {
            var counter = 0;
            for (var i = 0; i < packet.Length - bitCount; i++) if (packet[i]) counter++;
            var checksumString = Convert.ToString(counter, 2);
            checksumString = checksumString.PadLeft(bitCount, '0');
            var checksum = new BitArray(checksumString.Select(x => x == '1').ToArray());
            
            var packetChecksum = new BitArray(bitCount);
            for (var i = 0; i < bitCount; i++) packetChecksum[i] = packet[(packet.Length - bitCount) + i];
            return checksum.EqualsValue(packetChecksum);
        }

        public override string ToString()
        {
            return $"bit '1' sum with length: {bitCount}";
        }
    }
}