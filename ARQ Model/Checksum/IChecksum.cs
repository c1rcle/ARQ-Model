using System.Collections;

namespace ARQ_Model.Checksum
{
    public interface IChecksum
    {
        BitArray CalculateChecksum(BitArray packet);

        bool CheckChecksum(BitArray packet);
    }
}