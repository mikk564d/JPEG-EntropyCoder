using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    class HuffmanNode {

        private static LinkedList<LinkedList<string>> DHTLists; //Contains the values for each level of the tree

        private string Value;
        private string Address;
        private int Level;
        private bool Leaf;

        private HuffmanNode Left;
        private HuffmanNode Right;


        public HuffmanNode(string binaddr, string DHT=null) {

            if (DHT != null) {
                this.populateLists(DHT);
            }

            this.Level = binaddr.Length;
            this.Address = binaddr;

            this.makeMeLeaf();

            if (this.Leaf && this.Level < 16) {
                this.Left = new HuffmanNode(binaddr + "0");
                this.Right = new HuffmanNode(binaddr + "1");
            }
        }

        /// <summary>
        /// DHTLists is populated by creating a new sublist for every level in the huffmantree
        /// and adding any values that might be present for that level to that sublist.
        /// </summary>
        /// <param name="DHT">Must be a space separated string of individual hex-values.</param>
        public void populateLists(string DHT) {

            HuffmanNode.DHTLists = new LinkedList<LinkedList<string>> { };
            string[] dhtsplit = DHT.Split(' ');
            int valueIndex = 16;
            for (int i = 0; i < 16; i++) {

                int dhtamount = Convert.ToInt32(dhtsplit[i].ToString(), 16);

                LinkedList<string> valuesList = new LinkedList<string> { };
                for (int d = valueIndex; d < valueIndex + dhtamount; d++) {
                    valuesList.AddLast(dhtsplit[d]);
                }
                valueIndex += dhtamount;
                HuffmanNode.DHTLists.AddLast(valuesList);
            }

        }

        public string SearchFor(string binAddr) {
            //Takes a binary sequence by string and seraches for a value. 
            //If no leaf is found at that address, an empty string is returned.
            if (this.Leaf) {
                if (this.Address == binAddr) {
                    return this.Value;
                } else {
                    return "";
                }
            } else {
                string result;

                if (binAddr.Length <= this.Level) {
                    result = "";
                } else if (binAddr[this.Level] == '0') { // Go left 
                    result = this.Left.SearchFor(binAddr);
                } else {
                    result = this.Right.SearchFor(binAddr);
                }
                return result;
            }
        }

        public void printAddresses(out List<string> result) {
            result = new List<string> { };
            if (this.Leaf) {
                result.Add(string.Format("{0} - {1}", this.Address, this.Value));
            }

            if (this.Left != null) {
                this.Left.printAddresses(out result);
            }
            if (this.Right != null) {
                this.Right.printAddresses(out result);
            }

        }


        private LinkedList<string> levelList() {
            // returns the list of values for the treelevel of this node.
            return HuffmanNode.DHTLists.ElementAt(this.Level);
        }



        private void makeMeLeaf() {
            //Tries to convert this node into a leaf and assign it a value.
            if (this.Level - 1 >= 0 && this.levelList().Count() > 0) {

                this.Value = this.levelList().First.Value;
                this.Leaf = true;
                this.levelList().RemoveFirst();
            } 
        }

    }
}

