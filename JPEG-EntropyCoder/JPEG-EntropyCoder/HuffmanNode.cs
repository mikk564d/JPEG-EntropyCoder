using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    class HuffmanNode {
        static LinkedList<LinkedList<string>> DHTLists; //Contains the values for each level of the tree

        public string value { get; protected set; }
        public string addr { get; protected set; }
        public int lvl { get; protected set; }
        public bool leaf { get; protected set; }

        public HuffmanNode left;
        public HuffmanNode right;

        public HuffmanNode(string DHT) : this("", DHT) {

        }

        public HuffmanNode(string binaddr, string DHT) {
            this.lvl = binaddr.Length;
            this.addr = binaddr;
            DHT = DHT.Remove(0, 3);
            DHT = "00 " + DHT;

            if (this.lvl == 0) {
                this.populateLists(DHT);
            }

            if (!makeMeLeaf() && this.lvl < 16) {
                this.left = new HuffmanNode(binaddr + "0", DHT);
                this.right = new HuffmanNode(binaddr + "1", DHT);
            }
        }

        public string SearchFor(string binAddr, int index) {
            //Takes a binary sequence by string and seraches for a value. 
            //If no leaf is found at that address, an empty string is returned.
            if (this.leaf) {
                if (this.addr == binAddr) {
                    return this.value;
                } else {
                    return "";
                }
            } else {
                string result;

                if (binAddr.Length <= index) {
                    result = "";
                } else if (binAddr[index] == '0') { // Go left 
                    result = this.left.SearchFor(binAddr, index + 1);
                } else {
                    result = this.right.SearchFor(binAddr, index + 1);
                }
                return result;
            }
        }

        public void printAddresses() {
            if (this.leaf) {
                Console.WriteLine("{0} - {1}", this.addr, this.value);
            }

            if (this.left != null) {
                this.left.printAddresses();
            }
            if (this.right != null) {
                this.right.printAddresses();
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
            int valueIndex = 17;
            for (int i = 0; i < 17; i++) {

                int dhtamount = Convert.ToInt32(dhtsplit[i].ToString(), 16);

                LinkedList<string> valuesList = new LinkedList<string> { };
                for (int d = valueIndex; d < valueIndex + dhtamount; d++) {
                    valuesList.AddLast(dhtsplit[d]);
                }
                valueIndex += dhtamount;
                HuffmanNode.DHTLists.AddLast(valuesList);
            }

        }

        public LinkedList<string> levelList() {
            // returns the list of values for the treelevel of this node.
            return HuffmanNode.DHTLists.ElementAt(this.lvl);
        }



        public bool makeMeLeaf() {
            //Tries to convert this node into a leaf and assign it a value.
            //Returns true if successfull and false otherwise
            if (this.lvl - 1 >= 0 && this.levelList().Count() > 0) {

                this.value = this.levelList().First.Value;
                this.leaf = true;
                HuffmanNode.DHTLists.ElementAt(this.lvl).RemoveFirst();
                return true;
            } else {
                return false;
            }
        }

    }
}
}
