using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    class HuffmanNode {


        private string Value;
        private string Address;
        private int Level;
        private bool Leaf;

        public HuffmanNode Left;
        public HuffmanNode Right;


        public HuffmanNode(string binaddr) {
            this.Level = binaddr.Length;
            this.Address = binaddr;

            this.makeMeLeaf();

            if (this.Leaf && this.Level < 16) {
                this.Left = new HuffmanNode(binaddr + "0");
                this.Right = new HuffmanNode(binaddr + "1");
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

        public void printAddresses() {
            if (this.Leaf) {
                Console.WriteLine("{0} - {1}", this.Address, this.Value);
            }

            if (this.Left != null) {
                this.Left.printAddresses();
            }
            if (this.Right != null) {
                this.Right.printAddresses();
            }

        }


        public LinkedList<string> levelList() {
            // returns the list of values for the treelevel of this node.
            return HuffmanTree.DHTLists.ElementAt(this.Level);
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

