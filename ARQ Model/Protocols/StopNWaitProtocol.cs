using System.Collections.Generic;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{

    public class StopNWaitProtocol : Protocol
    {
        private int requestNumber;

        private readonly List<int> packetsAcquired;

        private Packet currentPacket;
        
        private bool? currentAcknowledgement;

        private bool transmissionFinished;

        public StopNWaitProtocol(int byteCount, int packetSize, IChecksum checksumGenerator)
            : base(byteCount, packetSize, checksumGenerator)
        {
            requestNumber = 0;
            packetsAcquired = new List<int>();
            currentPacket = null;
        }

        public override void StartSimulation()
        {
            using (FileWriter = new StreamWriter(Filename))
            {
                FileWriter.WriteLine($"Using {ChecksumGenerator}, " +
                                     $"packet count: {TransferData.Length}");
                packetsAcquired.Clear();
                currentAcknowledgement = false;
                transmissionFinished = false;
                NoiseGenerator.FlipProbability = FlipProbability;
                while (true)
                {
                    SenderTask();
                    if (transmissionFinished) break;
                    ReceiverTask();
                }
            }
        }

        protected override bool? ProcessPacket(Packet packet)
        {
            if (packet.PacketData == null)
            {
                FileWriter.WriteLine($"Packet #{packet.Index} was lost.");
                return null;
            }
            var check = ChecksumGenerator.CheckChecksum(packet.PacketData);
            var conclusion = check ? "correct" : "incorrect";
            FileWriter.WriteLine($"Packet #{packet.Index} received as {conclusion}:" +
                                 $" {packet.PacketData.ToDigitString()}");
            if (!check) return null;
            packetsAcquired.Add(packet.Index);
            return NoiseGenerator.GetRandomWithProbability(AckLossProbability) ? new bool?() : true;
        }

        protected override Packet SendPacket()
        {
            var transferPacket = ChecksumGenerator.CalculateChecksum(TransferData[requestNumber]);
            FileWriter.WriteLine($"Packet #{requestNumber} sent: {transferPacket.ToDigitString()}");
            transferPacket = NoiseGenerator.GenerateNoise(transferPacket);
            return new Packet(NoiseGenerator.GetRandomWithProbability(PacketLossProbability)
                ? null
                : transferPacket, requestNumber); 
        }

        private void SendFirstPacket()
        {
            FileWriter.WriteLine($"Transmitting packet #{requestNumber}.");
            {
                currentPacket = SendPacket();
            }
        }

        private void CorrectAckAcquired()
        {
            FileWriter.WriteLine($"ACK acquired for #{requestNumber}");
            if (requestNumber >= TransferData.Length)
            {
                transmissionFinished = true;
                return;
            }
            requestNumber++;
            currentPacket = SendPacket();
        }

        private void NullAckAcquired()
        {
            FileWriter.WriteLine("ACK timeout. Resending packet.");
            currentPacket = SendPacket();
        }

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

        private void ReceiverTask()
        {
            currentAcknowledgement = ProcessPacket(currentPacket);
        }
    }
}