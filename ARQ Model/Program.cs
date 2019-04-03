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
            var protocol = new GoBackNProtocol(8, new BitParity(), "result.txt", 3)
            {
                FlipProbability = 0.001d, PacketLossProbability = 0.05d, AckLossProbability = 0.1d
            };
            protocol.StartSimulation();
        }
    }
}