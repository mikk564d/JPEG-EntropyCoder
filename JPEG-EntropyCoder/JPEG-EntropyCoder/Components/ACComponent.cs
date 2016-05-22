using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Components {

    /// <summary>
    /// EntropyComponent is a class which contains information about an entropy decoded bit sequence. 
    /// This Component is clasified as an ACComponent.
    /// </summary>
    public class ACComponent : EntropyValueComponent {

        /// <summary>
        /// Builds an ACComponent.
        /// </summary>
        /// <param name="huffmanTreePath">The path through the Huffman tree</param>
        /// <param name="huffmanLeafHexValue">The byte which was returned by Huffman tree</param>
        /// <param name="amplitude">The bit sequence from RLE.</param>
        public ACComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue, BitVector16 amplitude) 
            : base(huffmanTreePath, huffmanLeafHexValue, amplitude) { }
    }
}
