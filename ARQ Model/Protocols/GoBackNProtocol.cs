using System;
using System.Collections;
using System.Collections.Generic;
using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{
    public class GoBackNProtocol : Protocol
    {
        private readonly Queue<Packet> currentWindow;

        private readonly List<int> packetsAcquired;

        private readonly int windowSize;

        private bool? currentAcknowledgement;

        private int requestNumber;

        private bool transmissionFinished;

        private int windowPacketsLeft;

        public GoBackNProtocol(int byteCount, IChecksum checksumGenerator, string filename, int windowSize)
            : base(byteCount, checksumGenerator, filename)
        {
            if (windowSize < byteCount) throw new ArgumentException("Wrong parameter!");

            this.windowSize = windowSize;
            requestNumber = 0;
            currentWindow = new Queue<Packet>();
            packetsAcquired = new List<int>();
        }

        public override void StartSimulation()
        {
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

            FileWriter.Close();
        }

        protected override Packet SendPacket()
        {
            var currentByte = TransferData[requestNumber];
            var transferPacket = ChecksumGenerator.CalculateChecksum(
                new BitArray(new[] {currentByte}));
            FileWriter.WriteLine($"Packet #{requestNumber} sent: {transferPacket.ToDigitString()}");
            transferPacket = NoiseGenerator.GenerateNoise(transferPacket);
            return new Packet(NoiseGenerator.GetRandomWithProbability(PacketLossProbability)
                ? null
                : transferPacket, requestNumber);
        }

        protected override bool? ProcessPacket(Packet packet)
        {
            if (packetsAcquired.Contains(packet.Index)) return true;
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

        private void SenderTask()
        {
            switch (currentAcknowledgement)
            {
                case null:
                {
                    FileWriter.WriteLine("ACK timeout. Resending packets.");
                    currentWindow.Clear();
                    if (requestNumber >= TransferData.Length)
                        windowPacketsLeft = TransferData.Length + windowSize - requestNumber;
                    else
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

                case false:
                {
                    currentWindow.Clear();
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

        private void ReceiverTask()
        {
            currentAcknowledgement = ProcessPacket(currentWindow.Dequeue());
        }
    }
}