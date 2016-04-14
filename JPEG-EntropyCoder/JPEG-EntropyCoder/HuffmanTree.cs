using System;
using System.Collections.Generic;
using System.Linq;

namespace JPEG_EntropyCoder {
    public class HuffmanTree : IHuffmanTree {

        public string DHT { get; }

        private HuffmanNode tree;

        public HuffmanTree(string DHT) {
            this.DHT = DHT;
            this.tree = new HuffmanNode("", DHT);
        }

        public string Find(string treePath) {
            return this.tree.SearchFor(treePath);
        }

        public List<string> PrintTree() {
            List<string> result = new List<string> { };
            this.tree.printAddresses(ref result);
            return result;
        }
    }
}
