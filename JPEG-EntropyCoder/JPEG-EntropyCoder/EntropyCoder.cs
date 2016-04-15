using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Exceptions;
using JPEG_EntropyCoder.Interfaces;

namespace JPEG_EntropyCoder {
    public class EntropyCoder : IEntropyCoder {
        public EntropyCoder(string path) {
            HuffmanTrees = new List<HuffmanTree>();
            EntropyComponents = new List<EntropyComponent>();
            FileHandler = new JPEGFileHandler(path);

            HuffmanTrees = BuildHuffmanTrees(FileHandler.DHT);
            foreach (HuffmanTree huffmanTree in HuffmanTrees) {
                Console.WriteLine("Huffmantree");
                foreach (string s in huffmanTree.PrintTree()) {
                    Console.WriteLine(s);
                }
            }
        }

        enum HuffmanTable {
            LumDC = 0, ChromDC = 1, LumAC = 2, ChromAC = 3
        }

        //private int CurrentIndex { get; set; }
        //private string BinaryData { get; set; }
        private JPEGFileHandler FileHandler { get; }
        private List<HuffmanTree> HuffmanTrees { get; }
        public List<EntropyComponent> EntropyComponents { get; set; }

        private List<HuffmanTree> BuildHuffmanTrees (string DHTFromFile) {
            string[] DHTValues = DHTFromFile.Split(' ');


            List<string> DHTs = new List<string>();
            int index = 0;
            for (int i = 0; i < 4; i++) {
                string dht = "";
                int count = 0;
                index++;
                int upperLimit = index + 16;
                for ( ;index < upperLimit; index++) {
                    string currentByte = DHTValues[index];
                    dht += currentByte + " ";
                    count += Convert.ToInt32(currentByte, 16);
                }

                

                upperLimit = count + index;
                for (; index < upperLimit; index++) {
                    dht += DHTValues[index] + " ";
                }
                DHTs.Add(dht);
            }

            foreach (string dhT in DHTs) {
                Console.WriteLine(dhT);
            }

            List<HuffmanTree> huffmanTrees = new List<HuffmanTree>();
            foreach (string table in DHTs) {
                huffmanTrees.Add(new HuffmanTree(table));
            }

            return huffmanTrees;
        }

        private string GetBinaryData(JPEGFileHandler extractor) {
            string data = extractor.CompressedImage;
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(data[i].ToString(), 16), 2).PadLeft(4, '0'));
            }

            return sBuilder.ToString();
        }

        public void Encode() {
            FileHandler.CompressedImage = GetReEncodedData();
        }

        public void Decode() {
            //CurrentIndex = 0;
            DecodeBinaryData();
        }

        public void Save(string path) {
            FileHandler.SaveFile(path);
        }


        private void DecodeBinaryData() {
            bool hitEOB = false;
            int currentIndex = 0;

            string binaryData = GetBinaryData(FileHandler);
            int luminensSubsamling = 4;
            int chrominensSubsampling = 2;

            while (currentIndex < binaryData.Length && binaryData.Length - currentIndex > 8) {
                //DecodeBlock(luminensSubsamling, "luminens");
                //DecodeBlock(chrominensSubsampling, "chrominens");

                int length;
                string toCheck;
                int count;
                for (int i = 0; i < luminensSubsamling; i++) { 
                    hitEOB = false;
                    length = binaryData.Length - currentIndex > 31 ? 31 : binaryData.Length - currentIndex;
                    toCheck = binaryData.Substring(currentIndex, length);
                    DecodeHuffmanHexValue(toCheck ,HuffmanTable.LumDC, true, out count);
                    currentIndex += count;
                    for (int j = 0; j < 63 && !hitEOB; j++) {
                        length = binaryData.Length - currentIndex > 31 ? 31 : binaryData.Length - currentIndex;
                        toCheck = binaryData.Substring(currentIndex, length);
                        hitEOB = DecodeHuffmanHexValue(toCheck, HuffmanTable.LumAC, false, out count);
                        currentIndex += count;
                    }
                }

                for (int i = 0; i < chrominensSubsampling; i++) {
                    hitEOB = false;
                    length = binaryData.Length - currentIndex > 31 ? 31 : binaryData.Length - currentIndex;
                    toCheck = binaryData.Substring(currentIndex, length);
                    DecodeHuffmanHexValue(toCheck, HuffmanTable.ChromDC, true, out count);
                    currentIndex += count;
                    for (int j = 0; j < 63 && !hitEOB; j++) {
                        length = binaryData.Length - currentIndex > 31 ? 31 : binaryData.Length - currentIndex;
                        toCheck = binaryData.Substring(currentIndex, length);
                        hitEOB = DecodeHuffmanHexValue(toCheck, HuffmanTable.ChromAC, false, out count);
                        currentIndex += count;
                    }
                }
            }
        }

        //private void DecodeBlock(string binaryData, int subsampling, string typeTable) {
        //    bool hitEOB = false;
        //    HuffmanTable DC;
        //    HuffmanTable AC;

        //    if (typeTable == "luminens") {
        //        DC = HuffmanTable.LumDC;
        //        AC = HuffmanTable.LumAC;
        //    } else {
        //        DC = HuffmanTable.ChromDC;
        //        AC = HuffmanTable.ChromAC;
        //    }
        //    int length;
        //    string toCheck;
        //    for (int i = 0; i < subsampling; i++) {
        //        length = binaryData.Length > 31 ? 31 : binaryData.Length - CurrentIndex;
        //        toCheck = binaryData.Substring(CurrentIndex, length);
        //        DecodeHuffmanHexValue(toCheck, DC, true);
        //        for (int j = 0; j < 63 || !hitEOB; j++) {
        //            length = binaryData.Length > 31 ? 31 : binaryData.Length - CurrentIndex;
        //            toCheck = binaryData.Substring(CurrentIndex, length);
        //            hitEOB = DecodeHuffmanHexValue(toCheck, AC, false);
        //        }
        //    }
        //}

        private bool DecodeHuffmanHexValue(string bitString ,HuffmanTable table, bool isDC, out int count) {
            string huffmanLeafHexValue;
            string amplitude;
            string huffmanTreePath;
            count = 0;

            int length = bitString.Length > 16 ? 16 : bitString.Length;
            string toCheck = bitString.Substring(0, length);
            GetHuffmanLeafHexValue(toCheck ,out huffmanTreePath, out huffmanLeafHexValue, table);
            count += huffmanTreePath.Length;

            if (huffmanLeafHexValue != "00" || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                //amplitude = GetAmplitude(huffmanLeafHexValue);
                int lenght = Convert.ToInt32(huffmanLeafHexValue[1].ToString(), 16);
                amplitude = bitString.Substring(huffmanTreePath.Length, lenght);
                count += amplitude.Length;

                if (amplitude == "") {
                    amplitude = "0";
                }

                if (isDC) {
                    EntropyComponents.Add(new DCComponent(huffmanTreePath, huffmanLeafHexValue, amplitude));
                } else {
                    EntropyComponents.Add(new ACComponent(huffmanTreePath, huffmanLeafHexValue, amplitude));
                }
            } else {
                EntropyComponents.Add(new EOBComponent(huffmanTreePath, huffmanLeafHexValue));
                return true;
            }

            return false;
        }

        private void GetHuffmanLeafHexValue(string bitString ,out string currentHuffmanTreePath, out string huffmanLeafHexValue, HuffmanTable table) {
            currentHuffmanTreePath = "";
            huffmanLeafHexValue = "";

            //while (huffmanLeafHexValue == "" && localCount < 16) {
            //    currentHuffmanTreePath += BinaryData[];
            //    huffmanLeafHexValue = HuffmanTrees[(int)table].Find(currentHuffmanTreePath);
            //}

            for (int i = 0; i < bitString.Length && huffmanLeafHexValue == ""; i++) {
                currentHuffmanTreePath += bitString[i];
                huffmanLeafHexValue = HuffmanTrees[(int)table].Find(currentHuffmanTreePath);
            }

            if (huffmanLeafHexValue == "") {
                throw new BinaryPathNotFoundInHuffmanTreeException($"The path {currentHuffmanTreePath} was not found in {table} HuffmanTree.");
            }
        }

        //private string GetAmplitude(string huffmanLeafHexValue) {
        //    string amplitude = "";
        //    int lenght = Convert.ToInt32(huffmanLeafHexValue[1].ToString(), 16);

        //    for (int i = 0; i < lenght; i++, CurrentIndex++) {
        //        amplitude += BinaryData[CurrentIndex];
        //    }
        //    return amplitude;
        //}

        private string GetReEncodedData() {
            string HexData = "";

            StringBuilder sBuilder = new StringBuilder();

            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafHexValue == "00") || entropyComponent is EOBComponent) {
                    sBuilder.Append(entropyComponent.HuffmanTreePath);
                } else {
                    sBuilder.Append(entropyComponent.HuffmanTreePath + entropyComponent.Amplitude);
                }
            }

            while (sBuilder.Length % 8 != 0) {
                sBuilder.Append("1");
            }

            string binaryData = sBuilder.ToString();

            sBuilder.Clear();

            for (int i = 0; i < binaryData.Length; i += 4) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(binaryData.Substring(i, 4), 2), 16));
            }

            HexData = sBuilder.ToString();

            return HexData;
        }
    }
}
