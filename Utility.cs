using System;
using System.Collections;
using System.Text;

namespace UtilityNamespace
{
    public static class Utility
    {

        public static Random myRandom = new Random();
        public static string BinToString(this BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = bits.Length - 1; i >= 0; --i)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }
        public static BitArray StringToBin(this string bits)
        {
            var a = new BitArray(bits.Length);

            for (int i = bits.Length - 1; i >= 0; --i)
                a[i] = bits[i] == '1' ? true : false;

            return a;
        }
    }
}
