using System.Collections.Generic;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{

    public class StopNWaitProtocol : Protocol
    {
        private int requestNumber;

        private readonly List<int> packetsAcquired;
        
        private bool? currentAcknowledgement;

        private bool transmissionFinished;

        public StopNWaitProtocol(int byteCount, IChecksum checksumGenerator, string filename) : base(byteCount, checksumGenerator, filename)
        {
            requestNumber = 0;
            packetsAcquired = new List<int>();
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
            throw new System.NotImplementedException();
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
        }

        private void CorrectAckAcquired()
        {
            FileWriter.WriteLine($"ACK acquired for #{requestNumber}");
            requestNumber++;
        }

        private void NullAckAcquired()
        {
            FileWriter.WriteLine("ACK timeout. Resending packet.");
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
    }
}