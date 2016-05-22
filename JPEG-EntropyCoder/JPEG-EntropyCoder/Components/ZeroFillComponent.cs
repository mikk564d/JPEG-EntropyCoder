using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Components {

    /// <summary>
    /// EntropyComponent is a class which contains information about an entropy decoded bit sequence. 
    /// This Component is clasified as a ZeroFillComponent.
    /// </summary>
    class ZeroFillComponent : EntropyComponent {

        /// <summary>
        /// Builds a ZeroFillComponent.
        /// </summary>
        /// <param name="huffmanTreePath">The path through the Huffman tree</param>
        /// <param name="huffmanLeafHexValue">The byte which was returned by Huffman tree</param>
        public ZeroFillComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue) 
            : base(huffmanTreePath, huffmanLeafHexValue) {
        }
    }
}
