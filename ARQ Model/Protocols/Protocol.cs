using System;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    public abstract class Protocol
    {
        protected Protocol(int byteCount, IChecksum checksumGenerator, string filename)
        {
            if (byteCount < 1) throw new ArgumentException("Wrong parameter!");

            FlipProbability = 0.01d;
            PacketLossProbability = 0.005d;
            AckLossProbability = 0.005d;

            TransferData = new byte[byteCount];
            NoiseGenerator = new UniformNoise(FlipProbability);
            ChecksumGenerator = checksumGenerator;
            FileWriter = new StreamWriter(filename);

            var numberGenerator = new Random();
            numberGenerator.NextBytes(TransferData);
        }

        public double FlipProbability { protected get; set; }

        public double PacketLossProbability { protected get; set; }

        public double AckLossProbability { protected get; set; }

        protected byte[] TransferData { get; }

        protected UniformNoise NoiseGenerator { get; }

        protected IChecksum ChecksumGenerator { get; }

        protected StreamWriter FileWriter { get; }

        public abstract void StartSimulation();

        protected abstract Packet SendPacket();

        protected abstract bool? ProcessPacket(Packet packet);
    }
}