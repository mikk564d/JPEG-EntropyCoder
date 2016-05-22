
using System;
using System.Collections;
using Utilities;

namespace JPEG_EntropyCoder.Components {

    /// <summary>
    /// EntropyComponent is a class which contains information about an entropy decoded bit sequence.
    /// </summary>
     public abstract class EntropyComponent {

        /// <summary>
        /// The path through the Huffman tree.
        /// </summary>
        public BitVector16 HuffmanTreePath { get; }

        /// <summary>
        /// The byte which was returned by Huffman tree.
        /// </summary>
        public byte HuffmanLeafByte { get; }

        /// <summary>
        /// Builds an EntropyComponent.
        /// </summary>
        /// <param name="huffmanTreePath">The path through the Huffman tree</param>
        /// <param name="huffmanLeafHexValue">The byte which was returned by Huffman tree</param>
        protected EntropyComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue) {
            HuffmanTreePath = huffmanTreePath;
            HuffmanLeafByte = huffmanLeafHexValue;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equals to <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>Returns true if equals</returns>
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

        /// <summary>
        /// Returns a value indicating whether this instance is equals to <paramref name="other"/>
        /// </summary>
        /// <param name="other">EntropyComponent to compare</param>
        /// <returns>Returns true if equals</returns>
        protected bool Equals(EntropyComponent other) {
            return HuffmanTreePath.Equals(other.HuffmanTreePath) && HuffmanLeafByte == other.HuffmanLeafByte;
        }

        /// <summary>
        /// Returns the Hash Code for this instance.
        /// </summary>
        /// <returns>Returns the Hash Code for this instance</returns>
        public override int GetHashCode() {
            unchecked {
                return ((HuffmanTreePath != null ? HuffmanTreePath.GetHashCode() : 0) * 397) ^ HuffmanLeafByte.GetHashCode();
            }
        }
     }
}
