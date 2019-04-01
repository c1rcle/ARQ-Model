using System;
using System.Collections;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    public class GoBackNProtocol
    {
        private readonly byte[] transferData;

        private readonly UniformNoise noiseGenerator;

        private readonly IChecksum checksumGenerator;

        private StreamWriter fileWriter;
        
        private int requestNumber;

        public GoBackNProtocol(int byteCount, IChecksum checksumGenerator)
        {
            this.checksumGenerator = checksumGenerator;
            requestNumber = 0;
            transferData = new byte[byteCount];
            noiseGenerator = new UniformNoise(0.01d);
            
            var numberGenerator = new Random();
            numberGenerator.NextBytes(transferData);
        }
        
        public void StartSimulation(string filename)
        {
            fileWriter = new StreamWriter(filename);
            while (requestNumber < transferData.Length)
            {
                ProcessPacket(SendPacket());
                requestNumber++;
            }
            fileWriter.Close();
        }

        private BitArray SendPacket()
        {
            var currentByte = transferData[requestNumber];
            var transferPacket = checksumGenerator.CalculateChecksum(
                new BitArray(new[] { currentByte }));
            fileWriter.WriteLine($"Packet #{requestNumber} sent: {transferPacket.ToDigitString()}");
            transferPacket = noiseGenerator.GenerateNoise(transferPacket);
            return transferPacket;
        }

        private void ProcessPacket(BitArray packet)
        {
            var conclusion = checksumGenerator.CheckChecksum(packet) ? "correct" : "incorrect";
            fileWriter.WriteLine($"Packet #{requestNumber} received as {conclusion}: {packet.ToDigitString()}");
        }
    }
}