
using System.Collections;

namespace JPEG_EntropyCoder.Components {
     public abstract class EntropyComponent {
        protected EntropyComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue) {
            HuffmanTreePath = huffmanTreePath;
            HuffmanLeafHexValue = huffmanLeafHexValue;
        }

        public BitArray HuffmanTreePath { get; }

        public byte HuffmanLeafHexValue { get; }      
    }
}
