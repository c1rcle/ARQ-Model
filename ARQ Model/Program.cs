using ARQ_Model.Checksum;
using ARQ_Model.Protocols;

namespace ARQ_Model
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var protocol = new GoBackNProtocol(64, new BitParity());
            protocol.StartSimulation("result.txt");
        }
    }
}