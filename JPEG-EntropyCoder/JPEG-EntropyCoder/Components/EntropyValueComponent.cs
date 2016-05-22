using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {

    /// <summary>
    /// EntropyComponent is a class which contains information about an entropy decoded bit sequence. 
    /// This Component is clasified as an EntropyValueComponent.
    /// </summary>
    public abstract class EntropyValueComponent : EntropyComponent{

        /// <summary>
        /// The bit sequence from RLE.
        /// </summary>
        public BitVector16 Amplitude { get; set; }

        /// <summary>
        /// Get and Set the LSB for this value.
        /// </summary>
        public bool LSB {
            get { return Amplitude[(byte)(Amplitude.Length - 1)]; }
            set { Amplitude[(byte)(Amplitude.Length - 1)] = value ; }
        }

        /// <summary>
        /// Builds an EntropyValueComponent.
        /// </summary>
        /// <param name="huffmanTreePath">The path through the Huffman tree</param>
        /// <param name="huffmanLeafHexValue">The byte which was returned by Huffman tree</param>
        /// <param name="amplitude">The bit sequence from RLE.</param>
        protected EntropyValueComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue, BitVector16 amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }
    }
}
