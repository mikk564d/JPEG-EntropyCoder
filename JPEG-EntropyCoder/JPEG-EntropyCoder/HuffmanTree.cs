using System;
using System.Collections.Generic;
using System.Linq;

namespace JPEG_EntropyCoder {
    class HuffmanTree : IHuffmanTree {
        public static LinkedList<LinkedList<string>> DHTLists; //Contains the values for each level of the tree

        public string DHT { get; }

        private HuffmanNode tree;

        public HuffmanTree(string DHT) {
            this.DHT = DHT;
            this.populateLists(DHT);

            this.tree = new HuffmanNode("");
        }

        /// <summary>
        /// DHTLists is populated by creating a new sublist for every level in the huffmantree
        /// and adding any values that might be present for that level to that sublist.
        /// </summary>
        /// <param name="DHT">Must be a space separated string of individual hex-values.</param>
        public void populateLists(string DHT) {

            HuffmanTree.DHTLists = new LinkedList<LinkedList<string>> { };
            string[] dhtsplit = DHT.Split(' ');
            int valueIndex = 17;
            for (int i = 0; i < 17; i++) {

                int dhtamount = Convert.ToInt32(dhtsplit[i].ToString(), 16);

                LinkedList<string> valuesList = new LinkedList<string> { };
                for (int d = valueIndex; d < valueIndex + dhtamount; d++) {
                    valuesList.AddLast(dhtsplit[d]);
                }
                valueIndex += dhtamount;
                HuffmanTree.DHTLists.AddLast(valuesList);
            }

        }

        public string Find(string treePath) {
            return this.tree.SearchFor(treePath);
        }

        public string PrintTree() {
            throw new NotImplementedException();
        }
    }
}
