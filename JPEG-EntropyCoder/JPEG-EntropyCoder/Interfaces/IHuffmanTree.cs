using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Interfaces {
    /// <summary>
    /// Wrapper class for building Huffman trees
    /// </summary>
    public interface IHuffmanTree {
        /// <summary>
        /// The DHT used in construction of the contained tree.
        /// </summary>
        byte[] DHT { get; }
        /// <summary>
        /// Attempts to find a leaf at the supplied path.
        /// </summary>
        /// <param name="treePath">Only the first 16 bits will be used for leaf address lookup.</param>
        /// <returns>The <see cref="byte"/> that is found at the <paramref name="treePath"/>.</returns>
        byte Find(BitVector16 treePath);
        /// <summary>
        /// Build a print friendly version of the leaves in the tree.
        /// </summary>
        /// <returns>A list of strings that are concatenations of the address and value of each leaf in the tree.</returns>
        List<string> PrintTree();
    }
}
