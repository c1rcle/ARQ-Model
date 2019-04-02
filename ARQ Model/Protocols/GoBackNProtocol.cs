using System;
using System.Collections;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    public class GoBackNProtocol : Protocol
    {
        private int requestNumber;

        public GoBackNProtocol(int byteCount, IChecksum checksumGenerator, string filename) 
            : base(byteCount, checksumGenerator, filename)
        {
            requestNumber = 0;
        }
        
        public override void StartSimulation()
        {
            while (requestNumber < TransferData.Length)
            {
                ProcessPacket(SendPacket());
                requestNumber++;
            }
            FileWriter.Close();
        }

        protected override BitArray SendPacket()
        {
            var currentByte = TransferData[requestNumber];
            var transferPacket = ChecksumGenerator.CalculateChecksum(
                new BitArray(new[] { currentByte }));
            FileWriter.WriteLine($"Packet #{requestNumber} sent: {transferPacket.ToDigitString()}");
            transferPacket = NoiseGenerator.GenerateNoise(transferPacket);
            return transferPacket;
        }

        protected override void ProcessPacket(BitArray packet)
        {
            var conclusion = ChecksumGenerator.CheckChecksum(packet) ? "correct" : "incorrect";
            FileWriter.WriteLine($"Packet #{requestNumber} received as {conclusion}: {packet.ToDigitString()}");
        }
    }
}