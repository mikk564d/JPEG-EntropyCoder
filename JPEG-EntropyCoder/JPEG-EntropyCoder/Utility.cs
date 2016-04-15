using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    public static class Utility {
        public static BitArray ReverseBitArray(BitArray bitArray) {

            for (int i = 0; i < bitArray.Count / 8; i++) {
                for (int k = i * 8, j = (1 + i) * 8 - 1, count = 0; count < 4; ++k, --j, count++) {
                    bool temp = bitArray[k];
                    bitArray[k] = bitArray[j];
                    bitArray[j] = temp;
                }
            }

            return bitArray;
        }

        public static bool CompareBitArray(BitArray ba1, BitArray ba2) {
            if (ba1.Length != ba2.Length) {
                return false;
            }

            for (int i = 0; i < ba1.Length; i++) {
                if (ba1[i] != ba2[i]) {
                    return false;
                }
            }

            return true;
        }
    }
}
