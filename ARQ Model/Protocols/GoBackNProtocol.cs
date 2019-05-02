using System;
using System.Collections.Generic;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    /// <summary>
    /// GoBackNProtocol implementation.
    /// </summary>
    public class GoBackNProtocol : Protocol
    {
        /// <summary>
        /// Current window of packets for the receiver.
        /// </summary>
        private readonly Queue<Packet> currentWindow;

        /// <summary>
        /// List of acquired packet indices by the receiver.
        /// </summary>
        private readonly List<int> packetsAcquired;

        /// <summary>
        /// Sender window size.
        /// </summary>
        private readonly int windowSize;

        /// <summary>
        /// Current ACK request that was sent by the receiver.
        /// </summary>
        private bool? currentAcknowledgement;

        /// <summary>
        /// Request number for the sender to keep track of packet order.
        /// </summary>
        private int requestNumber;

        /// <summary>
        /// Boolean used to stop simulation (every packet was sent).
        /// </summary>
        private bool transmissionFinished;

        /// <summary>
        /// Number of packets that can be transported in a given window.
        /// </summary>
        private int windowPacketsLeft;

        public GoBackNProtocol(int byteCount, int packetSize, IChecksum checksumGenerator, int windowSize)
            : base(byteCount, packetSize, checksumGenerator)
        {
            //Packet window size cannot be bigger or equal than the number of sequence numbers.
            if (windowSize >= TransferData.Length) throw new ArgumentException("Wrong parameter!");

            this.windowSize = windowSize;
            requestNumber = 0;
            currentWindow = new Queue<Packet>();
            packetsAcquired = new List<int>();
        }

        /// <inheritdoc />
        public override void StartSimulation()
        {
            if (Filename != null)
            {
                using (FileWriter = new StreamWriter(Filename))
                {
                    SimulationTask();
                }
            }
            else SimulationTask();
        }
        
        

        /// <inheritdoc />
        protected override Packet SendPacket()
        {
            //Generate a packet, append a checksum and pass it through a noise simulator.
            var unmodifiedPacket = ChecksumGenerator.CalculateChecksum(TransferData[requestNumber]);
            if (Filename != null) 
                FileWriter.WriteLine($"Packet #{requestNumber} sent: {unmodifiedPacket.ToDigitString()}");
            var transferPacket = NoiseGenerator.GenerateNoise(unmodifiedPacket);
            //Packet can be lost (it will be null then).
            return new Packet(NoiseGenerator.GetRandomWithProbability(PacketLossProbability)
                ? null
                : transferPacket, unmodifiedPacket, requestNumber);
        }

        /// <inheritdoc />
        protected override bool? ProcessPacket(Packet packet)
        {
            //If we already have that packet we do nothing.
            if (packetsAcquired.Contains(packet.Index)) return true;
            //If packet was corrupted we return null (it simulates a timeout).
            if (packet.PacketData == null)
            {
                if (Filename != null) FileWriter.WriteLine($"Packet #{packet.Index} was lost.");
                LostPacketCount++;
                return null;
            }

            //Check if packet is not corrupted and send an acknowledgement (it can be lost too).
            var check = ChecksumGenerator.CheckChecksum(packet.PacketData);
            if (Filename != null)
            {
                var conclusion = check ? "correct" : "incorrect";
                FileWriter.WriteLine($"Packet #{packet.Index} received as {conclusion}:" + 
                                     $" {packet.PacketData.ToDigitString()}");}
            if (!check)
            {
                CorruptedCount++;
                return null;
            }
            if (!packet.PacketData.EqualsValue(packet.PacketUnmodifiedData)) MisjudgementCount++;
            packetsAcquired.Add(packet.Index);
            return NoiseGenerator.GetRandomWithProbability(AckLossProbability) ? new bool?() : true;
        }

        /// <summary>
        /// Task that configures and starts the simulation.
        /// </summary>
        private void SimulationTask()
        {
            if (Filename != null) FileWriter.WriteLine($"Using {ChecksumGenerator}, window size: {windowSize}, " + 
                                                       $"packet count: {TransferData.Length}");
            //Clear the packets acquired list and flags.
            packetsAcquired.Clear();
            requestNumber = 0;
            currentAcknowledgement = false;
            transmissionFinished = false;
                
            //Clear statistics saved from previous simulation.
            CorruptedCount = 0;
            MisjudgementCount = 0;
            LostPacketCount = 0;
            LostAcknowledgementCount = 0;
                
            //We can set probability after creating an object by using its property.
            NoiseGenerator.FlipProbability = FlipProbability;
            while (true)
            {
                SenderTask();
                if (transmissionFinished) break;
                ReceiverTask();
            }
        }

        /// <summary>
        /// Simulates a sender process.
        /// </summary>
        private void SenderTask()
        {
            switch (currentAcknowledgement)
            {
                case false:
                    SendFirstWindow();
                    break;
                case null:
                    NullAckAcquired();
                    break;
                default:
                    CorrectAckAcquired();
                    break;
            }
        }

        /// <summary>
        /// Sender helper method. Sends first window.
        /// </summary>
        private void SendFirstWindow()
        {
            if (Filename != null) FileWriter.WriteLine($"Transmitting {windowSize} packets," +
                                                       $" starting from #{requestNumber}.");
            for (var i = 0; i < windowSize; i++)
            {
                currentWindow.Enqueue(SendPacket());
                requestNumber++;
            }
        }

        /// <summary>
        /// Sender helper method. In the event of a timeout it retransmits current window.
        /// </summary>
        private void NullAckAcquired()
        {
            if (Filename != null) FileWriter.WriteLine("ACK timeout. Resending packets.");
            LostAcknowledgementCount++;
            currentWindow.Clear();
            //Send as much packets as possible in a window.
            requestNumber -= windowSize;
            windowPacketsLeft = (TransferData.Length - requestNumber - 1) / windowSize > 0
                ? windowSize
                : TransferData.Length - requestNumber;
            for (var i = 0; i < windowPacketsLeft; i++)
            {
                currentWindow.Enqueue(SendPacket());
                requestNumber++;
            }
            requestNumber += windowSize - windowPacketsLeft;
        }

        /// <summary>
        /// Sender helper method. On correct Ack acquisition it advances the window by one and sends next packet.
        /// </summary>
        private void CorrectAckAcquired()
        {
            if (Filename != null) FileWriter.WriteLine($"ACK acquired for #{requestNumber - windowSize}");
            if (requestNumber >= TransferData.Length)
            {
                if (requestNumber - windowSize == TransferData.Length - 1) transmissionFinished = true;
                requestNumber++;
                return;
            }
            currentWindow.Enqueue(SendPacket());
            requestNumber++;
        }

        /// <summary>
        /// Simulates a receiver process.
        /// </summary>
        private void ReceiverTask()
        {
            //Get first packet that waits in the queue and process it.
            currentAcknowledgement = ProcessPacket(currentWindow.Dequeue());
        }
    }
}