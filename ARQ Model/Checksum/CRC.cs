using System;
using System.Collections;

namespace ARQ_Model.Checksum
{
   
    public class CRC : IChecksum
    {
        /// <inheritdoc />
        public BitArray PacketWithCRCSent(BitArray packet)
        {
            
            var CRC=1101b;
            var PacketToSend = new BitArray(packet.Length + CRC.Length);
            var PacketTemp = new BitArray(packet.Length + CRC.Length);
			int temp=0;
			for(var i=0;i<CRC.Length;i++)
			{if(packet[i]
			}	
			           
            
            for (var i = 0; i < packet.Length; i++) PacketToSend[i] = packet[i];
            
            for(var j=packet.Length; j<PacketToSend.Length;j++)
            {
            PacketToSend[j]= PacketTemp[j];
            }
            return PacketToSend;
            
        }

        /// <inheritdoc />
        public bool CheckCRC(BitArray packet)
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