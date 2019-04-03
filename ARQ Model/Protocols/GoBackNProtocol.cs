using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public GoBackNProtocol(int byteCount, IChecksum checksumGenerator, string filename, int windowSize)
            : base(byteCount, checksumGenerator, filename)
        {
            //Packet window cannot be smaller than 0.
            if (windowSize < 1) throw new ArgumentException("Wrong parameter!");

            this.windowSize = windowSize;
            requestNumber = 0;
            currentWindow = new Queue<Packet>();
            packetsAcquired = new List<int>();
        }

        /// <inheritdoc />
        public override void StartSimulation()
        {
            //Clear the packets acquired list and flags.
            packetsAcquired.Clear();
            currentAcknowledgement = false;
            transmissionFinished = false;
            //We can set probability after creating an object by using its property.
            NoiseGenerator.FlipProbability = FlipProbability;
            while (true)
            {
                SenderTask();
                if (transmissionFinished) break;
                ReceiverTask();
            }
            FileWriter.Close();
        }

        /// <inheritdoc />
        protected override Packet SendPacket()
        {
            //Generate a packet, append a checksum and pass it through a noise simulator.
            var currentByte = TransferData[requestNumber];
            var transferPacket = ChecksumGenerator.CalculateChecksum(
                new BitArray(new[] {currentByte}));
            FileWriter.WriteLine($"Packet #{requestNumber} sent: {transferPacket.ToDigitString()}");
            transferPacket = NoiseGenerator.GenerateNoise(transferPacket);
            //Packet can be lost (it will be null then).
            return new Packet(NoiseGenerator.GetRandomWithProbability(PacketLossProbability)
                ? null
                : transferPacket, requestNumber);
        }

        /// <inheritdoc />
        protected override bool? ProcessPacket(Packet packet)
        {
            //If we already have that packet we do nothing.
            if (packetsAcquired.Contains(packet.Index)) return true;
            //If packet was corrupted we return null (it simulates a timeout).
            if (packet.PacketData == null)
            {
                FileWriter.WriteLine($"Packet #{packet.Index} was lost.");
                return null;
            }

            //Check if packet is not corrupted and send an acknowledgement (it can be lost too).
            var check = ChecksumGenerator.CheckChecksum(packet.PacketData);
            var conclusion = check ? "correct" : "incorrect";
            FileWriter.WriteLine($"Packet #{packet.Index} received as {conclusion}:" +
                                 $" {packet.PacketData.ToDigitString()}");
            if (!check) return null;
            packetsAcquired.Add(packet.Index);
            return NoiseGenerator.GetRandomWithProbability(AckLossProbability) ? new bool?() : true;
        }

        /// <summary>
        /// Simulates a sender process.
        /// </summary>
        private void SenderTask()
        {
            switch (currentAcknowledgement)
            {
                //If there was a timeout we send all packets from the current window again.
                case null:
                {
                    FileWriter.WriteLine("ACK timeout. Resending packets.");
                    currentWindow.Clear();
                    //Used so that we won't resend additional packets when window reaches the end of data.
                    if (requestNumber >= TransferData.Length)
                        windowPacketsLeft = TransferData.Length + windowSize - requestNumber;
                    else
                    //Send as much packets as possible in a window.
                        windowPacketsLeft = TransferData.Length - requestNumber / windowSize > 0
                            ? windowSize
                            : TransferData.Length - requestNumber;

                    requestNumber -= windowSize;
                    for (var i = 0; i < windowPacketsLeft; i++)
                    {
                        currentWindow.Enqueue(SendPacket());
                        requestNumber++;
                    }
                    break;
                }

                //This is going to be called at the start of simulation.
                case false:
                {
                    windowPacketsLeft = TransferData.Length - requestNumber / windowSize > 0
                        ? windowSize
                        : TransferData.Length - requestNumber;
                    FileWriter.WriteLine($"Transmitting {windowPacketsLeft} packets, starting from #{requestNumber}.");
                    for (var i = 0; i < windowPacketsLeft; i++)
                    {
                        currentWindow.Enqueue(SendPacket());
                        requestNumber++;
                    }
                    break;
                }

                //ACK was acquired so we send the next packet and advance the window.
                default:
                {
                    FileWriter.WriteLine($"ACK acquired for #{requestNumber - windowSize}");
                    if (requestNumber >= TransferData.Length)
                    {
                        if (requestNumber - windowSize == TransferData.Length - 1) transmissionFinished = true;
                        requestNumber++;
                        return;
                    }

                    currentWindow.Enqueue(SendPacket());
                    requestNumber++;
                    break;
                }
            }
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