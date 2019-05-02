using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <param name="packetSize">Single packet maximum byte size.</param>
        /// <param name="checksumGenerator">Checksum calculation method for packets.</param>
        /// <exception cref="ArgumentException">Minimum number of bytes is 1.</exception>
        protected Protocol(int byteCount, int packetSize, IChecksum checksumGenerator)
        {
            //We can't transfer less than 1 byte. Packet size must be smaller than byte count.
            if (byteCount < 1 || packetSize > byteCount) throw new ArgumentException("Wrong parameter!");

            //Default property values.
            FlipProbability = 0.01d;
            PacketLossProbability = 0.005d;
            AckLossProbability = 0.005d;
            Filename = null;

            //Generate 'byteCount' random bytes for transfer.
            var numberGenerator = new Random();
            var data = new byte[byteCount];
            var fullPacketCount = byteCount / packetSize;
            numberGenerator.NextBytes(data);

            //Split array into individual packets.
            var packetList = new List<BitArray>();
            for (var i = 0; i < fullPacketCount; i++)
            {
                packetList.Add(new BitArray(data.Take(packetSize).ToArray()));
                data = data.Skip(packetSize).ToArray();
            }
            if (data.Length > 0) packetList.Add(new BitArray(data.Take(data.Length).ToArray()));

            //Initialize object properties.
            NoiseGenerator = new UniformNoise(FlipProbability);
            ChecksumGenerator = checksumGenerator;
            TransferData = packetList.ToArray();
        }

        /// <summary>
        /// Number of packets that were correctly assumed as corrupted.
        /// </summary>
        public int CorruptedCount { get; protected set; }

        /// <summary>
        /// Number of packets that were incorrectly treated as not corrupted by the checksum algorithm.
        /// </summary>
        public int MisjudgementCount { get; protected set; }

        /// <summary>
        /// Number of packets that were lost during transmission.
        /// </summary>
        public int LostPacketCount { get; protected set; }

        /// <summary>
        /// Number of acknowledgements that were lost during transmission.
        /// </summary>
        public int LostAcknowledgementCount { get; protected set; }

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
        /// Result file filename. If filename is not set logging is disabled.
        /// </summary>
        public string Filename { protected get; set; }

        /// <summary>
        /// Data that is going to be transferred during simulation.
        /// </summary>
        protected BitArray[] TransferData { get; }

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
        protected StreamWriter FileWriter { get; set; }

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