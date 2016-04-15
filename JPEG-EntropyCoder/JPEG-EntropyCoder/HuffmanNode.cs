using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    class HuffmanNode {

        private static LinkedList<LinkedList<byte>> DHTLists; //Contains the values for each level of the tree

        private byte Value { get; set; }
        private BitArray Address { get; set; }
        private int Level { get; set; }
        private bool Leaf { get; set; }

        private HuffmanNode LeftNode { get; set; }
        private HuffmanNode RightNode { get; set; }

        public HuffmanNode(BitArray binaddr, byte[] DHT = null) {

            if (DHT != null) {
                this.PopulateLists(DHT);
            }

            Level = binaddr.Length;
            Address = (BitArray)binaddr.Clone();

            MakeMeLeaf();

            if (!Leaf && Level < 16) {
                BitArray nextBinAdr = (BitArray)binaddr.Clone();

                nextBinAdr.Length += 1;
                nextBinAdr[nextBinAdr.Length - 1] = false;
                LeftNode = new HuffmanNode(nextBinAdr);
                nextBinAdr[nextBinAdr.Length - 1] = true;
                RightNode = new HuffmanNode(nextBinAdr);
            }
        }

        /// <summary>
        /// DHTLists is populated by creating a new sublist for every level in the huffmantree
        /// and adding any values that might be present for that level to that sublist.
        /// </summary>
        /// <param name="DHT">Must be a space separated string of individual hex-values.</param>
        public void PopulateLists(byte[] DHT) {

            DHTLists = new LinkedList<LinkedList<byte>> { };
            int valueIndex = 16;
            for (int i = 0; i < 16; i++) {

                int dhtamount = DHT[i];

                LinkedList<byte> valuesList = new LinkedList<byte> { };
                for (int d = valueIndex; d < valueIndex + dhtamount; d++) {
                    valuesList.AddLast(DHT[d]);
                }
                valueIndex += dhtamount;
                DHTLists.AddLast(valuesList);
            }

        }

        public byte SearchFor(BitArray binAddr) {
            //Takes a binary sequence by string and seraches for a value. 
            //If no leaf is found at that address, an empty string is returned.
            if (Leaf) {
                if (CompareBitArray(Address, binAddr)) {
                    return Value;
                } else {
                    return 0xFF;
                }
            } else {
                byte result;

                if (binAddr.Length <= Level) {
                    // TODO describe 0xFF
                    result = 0xFF;
                } else if (binAddr[Level] == false) { // Go left 
                    result = LeftNode.SearchFor(binAddr);
                } else {
                    result = RightNode.SearchFor(binAddr);
                }
                return result;
            }
        }

        public static bool CompareBitArray(BitArray ba1, BitArray ba2) {
            if (ba1.Length != ba2.Length) {
                return false;
            }

            for (int i = 0; i < ba1.Length; i++) {
                if (ba1[i] != ba2[i]) {
                    return false;
                }
            }

            return true;
        }

        public void PrintAddresses(ref List<string> result) {

            if (Leaf) {
                string addressStr = "";
                foreach (bool b in Address) {
                    addressStr += b ? "1" : "0";
                }

                result.Add($"{addressStr} - {Value}");
            }

            LeftNode?.PrintAddresses(ref result);
            RightNode?.PrintAddresses(ref result);
        }

        private LinkedList<byte> LevelList() {
            // returns the list of values for the treelevel of this node.
            return DHTLists.ElementAt(Level - 1);
        }

        private void MakeMeLeaf() {
            //Tries to convert this node into a leaf and assign it a value.
            if (Level > 0 && LevelList().Any()) {

                Value = LevelList().First.Value;
                Leaf = true;
                LevelList().RemoveFirst();
            }
        }
    }
}

