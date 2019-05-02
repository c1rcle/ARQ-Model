using System.Collections;
using System.Linq;
using System.Text;

namespace ARQ_Model.Utility
{
    /// <summary>
    /// Static class for extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension function for BitArray class. Allows for printing object contents using a string.
        /// </summary>
        /// <param name="packet">Object to be represented as string.</param>
        /// <returns>String of "1" and "0" representing an object.</returns>
        public static string ToDigitString(this BitArray packet)
        {
            var builder = new StringBuilder();
            foreach (bool bit in packet) builder.Append(bit ? "1" : "0");
            return builder.ToString();
        }

        /// <summary>
        /// Extension function for BitArray class. Allows for value comparision between objects.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static bool EqualsValue(this BitArray packet, BitArray compare)
        {
            var temp = new BitArray(packet);
            return temp.Xor(compare).OfType<bool>().All(x => !x);
        }
    }
}