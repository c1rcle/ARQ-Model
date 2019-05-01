using System;
using System.Data.HashFunction.CRC;
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
                Filename = "result.txt"
            };
            
            StartSimulationForN(protocol, 50);
            Console.WriteLine();

            var protocolStop = new StopAndWaitProtocol(1000, 1, new CyclicRedundancyCheck(CRCConfig.CRC8))
            {
                FlipProbability = 0.003d, PacketLossProbability = 0.005d, AckLossProbability = 0.005d, 
                Filename = "resultSNW.txt"
            };
            
            StartSimulationForN(protocolStop, 50);
        }

        private static void StartSimulationForN(Protocol protocol, int count)
        {
            for (var i = 0; i < count; i++)
            {
                protocol.StartSimulation();
                Console.WriteLine($"{protocol.CorruptedCount}, {protocol.MisjudgementCount}, " + 
                                  $"{protocol.LostPacketCount}, {protocol.LostAcknowledgementCount}");
            }
        }
    }
}