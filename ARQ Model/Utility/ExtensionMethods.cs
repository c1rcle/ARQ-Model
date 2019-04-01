using System.Collections;
using System.Text;

namespace ARQ_Model.Utility
{
    public static class ExtensionMethods
    {
        public static string ToDigitString(this BitArray packet)
        {
            var builder = new StringBuilder();
            foreach (bool bit in packet) builder.Append(bit ? "1" : "0");
            return builder.ToString();
        }
    }
}