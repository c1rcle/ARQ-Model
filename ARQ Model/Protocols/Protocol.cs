using System;
using System.Collections;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    public abstract class Protocol
    {
        protected byte[] TransferData { get; }
        
        protected UniformNoise NoiseGenerator { get; }
    
        protected IChecksum ChecksumGenerator { get; }

        protected StreamWriter FileWriter { get; }

        protected Protocol(int byteCount, IChecksum checksumGenerator, string filename)
        {
            ChecksumGenerator = checksumGenerator;
            TransferData = new byte[byteCount];
            NoiseGenerator = new UniformNoise(0.01d);
            FileWriter = new StreamWriter(filename);

            var numberGenerator = new Random();
            numberGenerator.NextBytes(TransferData);
        }

        public abstract void StartSimulation();

        protected abstract BitArray SendPacket();

        protected abstract void ProcessPacket(BitArray packet);
    }
}