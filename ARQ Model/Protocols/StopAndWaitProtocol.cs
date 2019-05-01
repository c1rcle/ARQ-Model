using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    ///<summary>
    ///StopNWait Protocol implementation.
    ///</summary>
    public class StopAndWaitProtocol : Protocol
    {
        /// <summary>
        /// Request number for the sender to keep track of packet order.
        /// </summary>
        private int requestNumber;

        /// <summary>
        /// List of acquired packet indices by the receiver.
        /// </summary>
        private readonly List<int> packetsAcquired;

        ///<summary>
        /// Current packet for the receiver.
        ///</summary>
        private Packet currentPacket;

        /// <summary>
        /// Current ACK request that was sent by the receiver.
        /// </summary>
        private bool? currentAcknowledgement;

        /// <summary>
        /// Boolean used to stop simulation when every packet was sent.
        /// </summary>
        private bool transmissionFinished;

        public StopAndWaitProtocol(int byteCount, int packetSize, IChecksum checksumGenerator)
            : base(byteCount, packetSize, checksumGenerator)
        {
            requestNumber = 0;
            packetsAcquired = new List<int>();
            currentPacket = null;
        }

        /// <inheritdoc />
        public override void StartSimulation()
        {
            using (FileWriter = new StreamWriter(Filename))
            {
                FileWriter.WriteLine($"Using {ChecksumGenerator}, " +
                                     $"packet count: {TransferData.Length}");
                //Clear flags and packets acquired list.                     
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
        }

        /// <inheritdoc />
        protected override Packet SendPacket()
        {
            //Generates the packet, appends the checksum and sends it through the noise generator.
            var unmodifiedPacket = ChecksumGenerator.CalculateChecksum(TransferData[requestNumber]);
            FileWriter.WriteLine($"Packet #{requestNumber} sent: {unmodifiedPacket.ToDigitString()}");
            var transferPacket = NoiseGenerator.GenerateNoise(unmodifiedPacket);
            //If packet is lost it becomes null.
            return new Packet(NoiseGenerator.GetRandomWithProbability(PacketLossProbability)
                ? null
                : transferPacket, unmodifiedPacket, requestNumber); 
        }
        
        /// <inheritdoc />
        protected override bool? ProcessPacket(Packet packet)
        {
            //If packet was corrupted we return null (it simulates a timeout).
            if (packet.PacketData == null)
            {
                FileWriter.WriteLine($"Packet #{packet.Index} was lost.");
                LostPacketCount++;
                return null;
            }

            //Check if packet is not corrupted and send an acknowledgement (it can be lost too).
            var check = ChecksumGenerator.CheckChecksum(packet.PacketData);
            var conclusion = check ? "correct" : "incorrect";
            FileWriter.WriteLine($"Packet #{packet.Index} received as {conclusion}:" +
                                 $" {packet.PacketData.ToDigitString()}");
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
        /// Sender helper method, sends the first packet.
        /// </summary>
        private void SendFirstPacket()
        {
            FileWriter.WriteLine($"Transmitting packet #{requestNumber}.");
            currentPacket = SendPacket();
        }

        /// <summary>
        /// Sender helper method. On correct Ack acquisition it advances the request number and sends next packet.
        /// </summary>
        private void CorrectAckAcquired()
        {
            FileWriter.WriteLine($"ACK acquired for #{requestNumber}");
            if (requestNumber == TransferData.Length - 1)
            {
                transmissionFinished = true;
                return;
            }
            requestNumber++;
            currentPacket = SendPacket();
        }

        /// <summary>
        /// Sender helper method. In the event of timeout it retransmits packet.
        /// </summary>
        private void NullAckAcquired()
        {
            FileWriter.WriteLine("ACK timeout. Resending packet.");
            LostAcknowledgementCount++;
            currentPacket = SendPacket();
        }

        /// <summary>
        /// Simulates a sender process.
        /// </summary>
        private void SenderTask()
        {
            switch (currentAcknowledgement)
            {
                case false:
                    SendFirstPacket();
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
        /// Simulates a receiver process.
        /// </summary>
        private void ReceiverTask()
        {
            currentAcknowledgement = ProcessPacket(currentPacket);
        }
    }
}