using ARQ_Model.Checksum;
using ARQ_Model.Protocols;

namespace ARQ_Model
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var protocol = new GoBackNProtocol(8, new BitParity(), "result.txt", 3)
            {
                FlipProbability = 0.001d, PacketLossProbability = 0.05d, AckLossProbability = 0.1d
            };
            protocol.StartSimulation();
        }
    }
}