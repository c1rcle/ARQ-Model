using System;
using System.Data.HashFunction.CRC;
using System.IO;
using ARQ_Model.Checksum;
using ARQ_Model.Protocols;

namespace ARQ_Model
{
    /// <summary>
    /// Program entry point class.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Entry program function.
        /// </summary>
        /// <param name="args">Program parameters.</param>
        private static void Main(string[] args)
        {
            //Make a new object of class GoBackNProtocol and initialize its properties.
            var protocol = new GoBackNProtocol(1000, 1, new BitParity(), 8)
            {
                FlipProbability = 0.005d, PacketLossProbability = 0.005d, AckLossProbability = 0.005d,
            };
            
            StartSimulationForN(protocol, 1000, "backNPar.txt");

            protocol.Filename = "testLog.txt";
            protocol.StartSimulation();

            var protocolStop = new StopAndWaitProtocol(1000, 1, new CyclicRedundancyCheck(CRCConfig.CRC8))
            {
                FlipProbability = 0.003d, PacketLossProbability = 0.005d, AckLossProbability = 0.005d,
            };
            
            StartSimulationForN(protocolStop, 1000, "stopCRC.txt");
        }

        /// <summary>
        /// Reruns the simulation 'count' times and writes data to a file.
        /// </summary>
        /// <param name="protocol">Protocol that is going to be used.</param>
        /// <param name="count">How many times the simulation is going to be run.</param>
        /// <param name="filename">Output filename.</param>
        private static void StartSimulationForN(Protocol protocol, int count, string filename)
        {
            if (filename == null) return;
            using (var writer = new StreamWriter(filename))
            {
                for (var i = 0; i < count; i++)
                {
                    protocol.StartSimulation();
                    writer.WriteLine($"{protocol.CorruptedCount}, {protocol.MisjudgementCount}, " +
                                      $"{protocol.LostPacketCount}, {protocol.LostAcknowledgementCount}");
                }
            }
        }
    }
}