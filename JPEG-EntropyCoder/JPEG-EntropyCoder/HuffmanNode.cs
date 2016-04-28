using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Utilities;

namespace JPEG_EntropyCoder {
    class HuffmanNode {

        private static LinkedList<LinkedList<byte>> DHTLists; //Contains the values for each level of the tree

        private byte Value { get; set; }
        private BitArray Address { get; set; }
        private int Level { get; set; }
        private bool Leaf { get; set; }

        private HuffmanNode LeftNode { get; set; }
        private HuffmanNode RightNode { get; set; }


        /// <summary>
        /// Builds the static stack if necesarry and start the recursive construction of the tree.
        /// </summary>
        /// <param name="binaddr">Should contain the expected address of this node. Should be empty if DHT is not null.</param>
        /// <param name="DHT">An array representation of a DHT as it appears in a JPEG file.</param>
        public HuffmanNode(BitArray binaddr, byte[] DHT = null) {
            Contract.Requires(DHT == null || (DHT != null && (binaddr == null || binaddr.Length == 0)));

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
        private void PopulateLists(byte[] DHT) {

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

        /// <summary>
        /// Takes a binary sequence by string and seraches for a value.
        /// </summary>
        /// <param name="binAddr">The address to search for. Can be incomplete.</param>
        /// <returns>If a matching leaf is found it's value is returned. If no leaf is found, 0XFF is returned.</returns>
        public byte SearchFor(BitArray binAddr) {
            Contract.Requires<ArgumentNullException>(binAddr != null);
            if (Leaf) {
                if (BitArrayUtilities.CompareBitArray(Address, binAddr)) {
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


        /// <summary>
        /// Adds the value and address of all leafs recursively to the referenced list of strings
        /// </summary>
        /// <param name="result">Must be passed as reference. No order in result is guaranteed.</param>
        public void PrintAddresses(ref List<string> result) {
            Contract.Requires<ArgumentNullException>(result != null);
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

        /// <summary>
        /// Finds the appropriate list of DHT values for the level of this object.
        /// </summary>
        /// <returns>The list of byte values at the calling objects level in the tree</returns>
        private LinkedList<byte> LevelList() {
            // returns the list of values for the treelevel of this node.
            return DHTLists.ElementAt(Level - 1);
        }

        /// <summary>
        /// Tries to convert this node into a leaf and assign it a value.
        /// If successfull the assigned value is also removed from the DHTLists
        /// To check if the method was successfull, check the Leaf property of the object.
        /// </summary>
        private void MakeMeLeaf() {
            if (Level > 0 && LevelList().Any()) {

                Value = LevelList().First.Value;
                Leaf = true;
                LevelList().RemoveFirst();
            }
        }
    }
}

