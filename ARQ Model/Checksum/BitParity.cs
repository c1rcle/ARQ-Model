using System;
using System.Collections;

namespace ARQ_Model.Checksum
{
    /// <summary>
    /// Bit parity error code implementation.
    /// </summary>
    public class BitParity : IChecksum
    {
        /// <inheritdoc />
        public BitArray CalculateChecksum(BitArray packet)
        {
            //Parity bit is calculated by counting all "1" in a packet and then performing a mod2 operation.
            var counter = 0;
            var newPacket = new BitArray(packet.Length + 1);
            foreach (bool bit in packet) if (bit) counter++;

            counter %= 2;
            for (var i = 0; i < packet.Length; i++) newPacket[i] = packet[i];
            newPacket[newPacket.Length - 1] = Convert.ToBoolean(counter);
            return newPacket;
        }

        /// <inheritdoc />
        public bool CheckChecksum(BitArray packet)
        {
            //Calculate parity bit again and check the last bit of a packet.
            var counter = 0;
            for (var i = 0; i < packet.Length - 1; i++) if (packet[i]) counter++;
            counter %= 2;
            return Convert.ToBoolean(counter) == packet[packet.Length - 1];
        }

        public override string ToString()
        {
            return "bit parity checksum";
        }
    }
}