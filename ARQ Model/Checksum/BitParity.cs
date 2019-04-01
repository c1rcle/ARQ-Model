using System;
using System.Collections;

namespace ARQ_Model.Checksum
{
    public class BitParity : IChecksum
    {
        public BitArray CalculateChecksum(BitArray packet)
        {
            var counter = 0;
            var newPacket = new BitArray(packet.Length + 1);
            foreach (bool bit in packet) if (bit) counter++;

            counter %= 2;
            for (var i = 0; i < packet.Length; i++) newPacket[i] = packet[i];
            newPacket[newPacket.Length - 1] = Convert.ToBoolean(counter);
            return newPacket;
        }

        public bool CheckChecksum(BitArray packet)
        {
            var counter = 0;
            for (var i = 0; i < packet.Length - 1; i++) if (packet[i]) counter++;
            counter %= 2;
            return Convert.ToBoolean(counter) == packet[packet.Length - 1];
        }
    }
}