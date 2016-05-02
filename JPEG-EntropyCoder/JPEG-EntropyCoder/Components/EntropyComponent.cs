
using System;
using System.Collections;
using Utilities;

namespace JPEG_EntropyCoder.Components {
     public abstract class EntropyComponent {
        public BitArray HuffmanTreePath { get; }
        public byte HuffmanLeafByte { get; }

        protected EntropyComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue) {
            HuffmanTreePath = huffmanTreePath;
            HuffmanLeafByte = huffmanLeafHexValue;
        }

        public override bool Equals(object obj) {
             if (ReferenceEquals(null, obj)) {
                 return false;
             }
             if (ReferenceEquals(this, obj)) {
                 return true;
             }
             if (obj.GetType() != this.GetType()) {
                 return false;
             }
             return Equals((EntropyComponent) obj);
         }

        protected bool Equals(EntropyComponent other) {
            return BitArrayUtilities.CompareBitArray(HuffmanTreePath, other.HuffmanTreePath) && HuffmanLeafByte == other.HuffmanLeafByte;
        }

        public override int GetHashCode() {
            unchecked {
                return ((HuffmanTreePath != null ? HuffmanTreePath.GetHashCode() : 0) * 397) ^ HuffmanLeafByte.GetHashCode();
            }
        }
     }
}
