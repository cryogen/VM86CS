using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace x86CS
{
    public static class Util
    {
        public static int CountSet(this BitArray bits)
        {
            int count = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    count++;
            }

            return count;
        }
    }
}
