using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Utility
    {
        public static void Clear(this byte[] b) // Clears any byte-array
        {
            for(int i = 0; i < b.Length; ++i)
                b[i] = 0;
        }

    }
}
