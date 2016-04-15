using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    public static class Utility {
        public static BitArray ReverseBitArray(BitArray bitArray) {

            int len = bitArray.Count;
            BitArray a = new BitArray(bitArray);
            BitArray b = new BitArray(bitArray);

            for (int i = 0, j = len - 1; i < len; ++i, --j) {
                a[i] = a[i] ^ b[j];
                b[j] = a[i] ^ b[j];
                a[i] = a[i] ^ b[j];
            }

            return a;
        }
    }
}
