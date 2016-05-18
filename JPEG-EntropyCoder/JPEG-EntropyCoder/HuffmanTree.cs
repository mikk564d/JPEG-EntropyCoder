using JPEG_EntropyCoder.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;


namespace JPEG_EntropyCoder {
    /// <summary>
    /// Wrapper class for building Huffman trees
    /// </summary>
    public class HuffmanTree : IHuffmanTree {
        /// <summary>
        /// The DHT used in construction of the contained tree.
        /// </summary>
        public byte[] DHT { get; }
        private HuffmanNode Root { get; }

        /// <summary>
        /// Manages the construction of a Huffmantree.
        /// The class performs no check to ensure the resulting tree is valid.
        /// </summary>
        /// <param name="DHT">An array representation of a DHT as it appears in a JPEG file.</param>
        public HuffmanTree(byte[] DHT) {
            Contract.Requires<ArgumentNullException>(DHT != null);
            Contract.Requires<ArgumentException>(DHT.Length > 0);
            this.DHT = DHT;
            Root = new HuffmanNode(new SimpleBitVector16(), DHT);
        }

        /// <summary>
        /// Attempts to find a leaf at the supplied path.
        /// </summary>
        /// <param name="treePath">Only the first 16 bits will be used for leaf address lookup.</param>
        /// <returns></returns>
        public byte Find(SimpleBitVector16 treePath) {
            Contract.Requires<ArgumentNullException>(treePath != null);
            
            return Root.SearchFor(treePath);
        }

        /// <summary>
        /// Build a print friendly version of the leaves in the tree.
        /// </summary>
        /// <returns>A list of strings that are concatenations of the address and value of each leaf in the tree.</returns>
        public List<string> PrintTree() {
            List<string> result = new List<string> { };
            Root.PrintAddresses(ref result);
            return result;
        }
    }
}
