using System;
using System.Collections;

namespace ARQ_Model.Checksum
{
   
    public class CRC : IChecksum
    {
        /// <inheritdoc />
        public BitArray PacketWithCRCSent(BitArray packet)
        {
            int crc_size;
            var CRC=new BitArray(crc_size);
            var PacketToSend = new BitArray(packet.Length + CRC.Length-1);//to bedzie pakiet wyjsciowy
            var PacketTemp = new BitArray(packet.Length + CRC.Length-1);//to bedzie zerowany pakiet przez operacje XOR
			
            for (var i=0;i<packet.length;i++)//poczatek tymczasowego ma byc taki jak wejsciowy pakiet
            {
                PacketTemp[i]=packet[i];
            }
            for(var i=packet.length;i<PacketTemp.length-1;i++)
            {
                PacketTemp[i]=0;//a na koncu maja byc zera
            }
            
            int indeks=0;//indeksy w tablicy tymczasowej do rozpoczecia xorowania (petla)
            if(PacketTemp[indeks]!=0)
            {temp=0;//wewnetrzny licznik petli xorujacej
            for(var i=0;i<CRC.Length;i++)
			    {
                    PacketTemp[temp]=PacketTemp[temp]^CRC[i];
                    temp++;
                }
                indeks++;
            }
            else 
            indeks++;
			           
            
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
            int crc_size;
            var CRC=new BitArray(crc_size);
            var PacketTemp = new BitArray(packet.Length + CRC.Length-1);//to bedzie zerowany pakiet przez operacje XOR
			
            for (var i=0;i<packet.length;i++)//poczatek tymczasowego ma byc taki jak wejsciowy pakiet
            {
                PacketTemp[i]=packet[i];
            }
            for(var i=packet.length;i<PacketTemp.length-1;i++)
            {
                PacketTemp[i]=0;//a na koncu maja byc zera
            }
            
            int indeks=0;//indeksy w tablicy tymczasowej do rozpoczecia xorowania (petla)
            if(PacketTemp[indeks]!=0)
            {temp=0;//wewnetrzny licznik petli xorujacej
            for(var i=0;i<CRC.Length;i++)
			    {
                    PacketTemp[temp]=PacketTemp[temp]^CRC[i];
                    temp++;
                }
                indeks++;
            }
            else 
            indeks++;
            bool num=true;//czy ostatnie wartosci po xorowaniu sa zerami(czy dobrze odebrano sygnal)
            for(int i=packet.size;i<PacketTemp.size;i++)
            {
                if(PacketTemp) num=false;
            }
            return num;
        }

       /*  public override string ToString()
        {
            return "bit parity checksum";
        }*/
    }
}