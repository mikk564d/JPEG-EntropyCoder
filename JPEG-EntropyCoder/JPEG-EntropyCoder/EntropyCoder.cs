using System;
using System.Collections;
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
            BinaryData = GetBinaryData(FileHandler);
            //new byte[] { 0x00, 0x00, 0x03, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x03, 0x04, 0x01, 0x00, 0x07 }

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

        private BitArray BinaryData { get; set; }
        private JPEGFileHandler FileHandler { get; }
        private List<HuffmanTree> HuffmanTrees { get; }
        public List<EntropyComponent> EntropyComponents { get; set; }

        private List<HuffmanTree> BuildHuffmanTrees(byte[] DHTFromFile) {
            List<byte[]> DHTs = new List<byte[]>();
            int index = 0;
            for (int i = 0; i < 4; i++) {
                List<byte> dht = new List<byte>();
                int count = 0;
                index++;
                for (int j = 0; j < 16; index++, j++) {
                    dht.Add(DHTFromFile[index]);
                    count += Convert.ToInt32(dht[j]);
                }

                for (int k = 0; k < count; index++, k++) {
                    dht.Add(DHTFromFile[index]);
                }
                DHTs.Add(dht.ToArray());
            }

            foreach (byte[] bytes in DHTs) {
                foreach (byte b in bytes) {
                    Console.Write(b + " ");
                }
                Console.WriteLine();
            }

            List<HuffmanTree> huffmanTrees = new List<HuffmanTree>();
            foreach (byte[] table in DHTs) {
                huffmanTrees.Add(new HuffmanTree(table));
            }

            return huffmanTrees;
        }

        private BitArray GetBinaryData(JPEGFileHandler extractor) {
            BitArray binData = new BitArray(extractor.CompressedImage);
            binData = Utility.ReverseBitArray(binData);

            for (int i = 0, j = binData.Count - 1; i < binData.Count / 2; i++, j--) {
                bool temp = binData[i];
                binData[i] = binData[j];
                binData[j] = temp;
            }

            return binData;
        }

        public void Encode() {
            //FileHandler.CompressedImage = GetReEncodedData();
        }

        public void Decode() {
            //CurrentIndex = 0;
            DecodeBinaryData();
        }

        public void Save(string path) {
            FileHandler.SaveFile(path);
        }


        private void DecodeBinaryData() {
            int luminensSubsamling = 1;
            int chrominensSubsampling = 2;

            while (BinaryData.Count >= 8) {
                DecodeBlock(luminensSubsamling, "luminens");
                DecodeBlock(chrominensSubsampling, "chrominens");
            }
        }

        private void DecodeBlock(int subsampling, string typeTable) {
            bool hitEOB = false;
            HuffmanTable DC;
            HuffmanTable AC;

            if (typeTable == "luminens") {
                DC = HuffmanTable.LumDC;
                AC = HuffmanTable.LumAC;
            } else {
                DC = HuffmanTable.ChromDC;
                AC = HuffmanTable.ChromAC;
            }

            for (int i = 0; i < subsampling; i++) {
                DecodeHuffmanHexValue(DC, true);
                for (int j = 0; j < 63 && !hitEOB; j++) {
                    hitEOB = DecodeHuffmanHexValue(AC, false);
                }
            }
        }

        private bool DecodeHuffmanHexValue(HuffmanTable table, bool isDC) {
            byte huffmanLeafHexValue;
            BitArray amplitude = new BitArray(16);
            BitArray huffmanTreePath;


            GetByteFromHuffmantree(out huffmanTreePath, out huffmanLeafHexValue, table);
            BinaryData.Length -= huffmanTreePath.Length;

            if (huffmanLeafHexValue != 0x00 || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                int length = huffmanLeafHexValue % 16;
                for (int i = BinaryData.Count - 1, j = 0; j < length; i--, j++) {
                    amplitude[j] = BinaryData[i];
                }
                BinaryData.Length -= length;

                if (amplitude.Count == 0) {
                    amplitude.Length += 1;
                    amplitude[0] = false;
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

        private void GetByteFromHuffmantree(out BitArray currentHuffmanTreePath, out byte huffmanLeafHexValue, HuffmanTable table) {
            currentHuffmanTreePath = new BitArray(16);
            huffmanLeafHexValue = 0xFF;

            for (int i = 0, j = BinaryData.Count - 1; i < 16 && huffmanLeafHexValue == 0xFF; i++, j--) {
                currentHuffmanTreePath[i] = BinaryData[j];
                huffmanLeafHexValue = HuffmanTrees[(int)table].Find(currentHuffmanTreePath);
            }

            if (huffmanLeafHexValue == 0xFF) {
                throw new BinaryPathNotFoundInHuffmanTreeException($"The path {currentHuffmanTreePath} was not found in {table} HuffmanTree.");
            }
        }

        //private string GetReEncodedData() {
        //    string HexData = "";

        //    StringBuilder sBuilder = new StringBuilder();

        //    foreach (EntropyComponent entropyComponent in EntropyComponents) {
        //        if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafHexValue == "00") || entropyComponent is EOBComponent) {
        //            sBuilder.Append(entropyComponent.HuffmanTreePath);
        //        } else {
        //            sBuilder.Append(entropyComponent.HuffmanTreePath + ((ValueComponent)entropyComponent).Amplitude);
        //        }
        //    }

        //    while (sBuilder.Length % 8 != 0) {
        //        sBuilder.Append("1");
        //    }

        //    string binaryData = sBuilder.ToString();

        //    sBuilder.Clear();

        //    for (int i = 0; i < binaryData.Length; i += 4) {
        //        sBuilder.Append(Convert.ToString(Convert.ToInt32(binaryData.Substring(i, 4), 2), 16));
        //    }

        //    HexData = sBuilder.ToString();

        //    return HexData;
        //}
    }
}
