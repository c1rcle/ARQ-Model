using ARQ_Model.Checksum;
using ARQ_Model.Utility;

namespace ARQ_Model.Protocols
{

    public class StopNWaitProtocol : Protocol
    {
        public StopNWaitProtocol(int byteCount, IChecksum checksumGenerator, string filename) : base(byteCount, checksumGenerator, filename)
        {
        }

        public override void StartSimulation()
        {
            throw new System.NotImplementedException();
        }

        protected override bool? ProcessPacket(Packet packet)
        {
            throw new System.NotImplementedException();
        }

        protected override Packet SendPacket()
        {
            throw new System.NotImplementedException();
        }
    }
}