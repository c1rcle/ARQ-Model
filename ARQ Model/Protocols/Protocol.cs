using System;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    /// <summary>
    /// Protocol abstract class.
    /// </summary>
    public abstract class Protocol
    {
        /// <summary>
        /// Constructor for the Protocol abstract class.
        /// </summary>
        /// <param name="byteCount">Number of bytes that are going to be sent.</param>
        /// <param name="checksumGenerator">Checksum calculation method for packets.</param>
        /// <param name="filename">"Name of simulation result file."</param>
        /// <exception cref="ArgumentException">Minimum number of bytes is 1.</exception>
        protected Protocol(int byteCount, IChecksum checksumGenerator, string filename)
        {
            //We can't transfer less than 1 byte.
            if (byteCount < 1) throw new ArgumentException("Wrong parameter!");

            //Default probability values.
            FlipProbability = 0.01d;
            PacketLossProbability = 0.005d;
            AckLossProbability = 0.005d;

            //Initialize object properties.
            TransferData = new byte[byteCount];
            NoiseGenerator = new UniformNoise(FlipProbability);
            ChecksumGenerator = checksumGenerator;
            FileWriter = new StreamWriter(filename);

            //Generate 'byteCount' random bytes for transfer.
            var numberGenerator = new Random();
            numberGenerator.NextBytes(TransferData);
        }

        /// <summary>
        /// Probability of bit flip during transmission.
        /// </summary>
        public double FlipProbability { protected get; set; }

        /// <summary>
        /// Probability of losing a packet during transmission.
        /// </summary>
        public double PacketLossProbability { protected get; set; }

        /// <summary>
        /// Probability of losing an acknowledgment during transmission.
        /// </summary>
        public double AckLossProbability { protected get; set; }

        /// <summary>
        /// Data that is going to be transferred during simulation.
        /// </summary>
        protected byte[] TransferData { get; }

        /// <summary>
        /// Uniform noise generator for noise simulation.
        /// </summary>
        protected UniformNoise NoiseGenerator { get; }

        /// <summary>
        /// Object implementing the IChecksum interface (see IChecksum for details).
        /// </summary>
        protected IChecksum ChecksumGenerator { get; }

        /// <summary>
        /// StreamWriter used to write data to a file.
        /// </summary>
        protected StreamWriter FileWriter { get; }

        /// <summary>
        /// Starts simulation and writes data to a file.
        /// </summary>
        public abstract void StartSimulation();

        /// <summary>
        /// Generates a packet (appends a checksum) that goes through a simulated medium.
        /// </summary>
        /// <returns>Packet passed through a NoiseGenerator.</returns>
        protected abstract Packet SendPacket();

        /// <summary>
        /// Checks whether packet was transmitted correctly.
        /// </summary>
        /// <param name="packet">Packet passed through a NoiseGenerator.</param>
        /// <returns>null - ACK lost simulation or incorrect packet/lost packet, true - correct ACK.</returns>
        protected abstract bool? ProcessPacket(Packet packet);
    }
}