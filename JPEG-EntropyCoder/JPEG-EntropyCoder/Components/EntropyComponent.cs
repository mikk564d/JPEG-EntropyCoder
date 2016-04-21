
using System.Collections;

namespace JPEG_EntropyCoder.Components {
     public abstract class EntropyComponent {
        protected EntropyComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue) {
            HuffmanTreePath = huffmanTreePath;
            HuffmanLeafByte = huffmanLeafHexValue;
        }

        public BitArray HuffmanTreePath { get; }

        public byte HuffmanLeafByte { get; }      
    }
}
