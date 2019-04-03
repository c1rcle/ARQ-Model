using System.Collections;

namespace ARQ_Model.Utility
{
    public class Packet
    {
        public Packet(BitArray packetData, int index)
        {
            PacketData = packetData;
            Index = index;
        }

        public BitArray PacketData { get; }

        public int Index { get; }
    }
}