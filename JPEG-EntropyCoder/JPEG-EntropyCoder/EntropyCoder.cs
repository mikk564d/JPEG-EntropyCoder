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
            RunLengthCoder = new RunLengthCoder();
            HuffmanTrees = new List<HuffmanTree>();
            EntropyComponents = new List<EntropyComponent>();
            FileHandler = new JPEGFileHandler(path);

            BuildHuffmanTrees(FileHandler);
            GetBinaryData(FileHandler);
        }

        enum HuffmanTable {
            LumDC = 0, LumAC = 1, ChromDC = 2, ChromAC = 3
        }

        private int CurrentIndex { get; set; }
        private string BinaryData { get; set; }
        private JPEGFileHandler FileHandler { get; }
        private RunLengthCoder RunLengthCoder { get; }
        private List<HuffmanTree> HuffmanTrees { get; }
        public List<EntropyComponent> EntropyComponents { get; set; }

        private void BuildHuffmanTrees(JPEGFileHandler fileHandler) {
            string DHT = fileHandler.DHT;

            List<string> DHTs = new List<string>();
            int index = 0;
            for (int i = 0; i < 4; i++) {
                string dht = "";
                int count = 0;
                index++;
                int upperLimit = index + 16;
                for ( ;index < upperLimit; index += 2) {
                    string currentByte = DHT[index].ToString() + DHT[index + 1].ToString();
                    dht += currentByte;
                    count += Convert.ToInt32(currentByte, 16);
                }
                upperLimit = count + index;
                for (; index < upperLimit; index += 2) {
                    dht += DHT[index] + DHT[index + 1];
                }
                DHTs[i] = dht;
            }

            foreach (string dht in DHTs) {
                Console.WriteLine(dht);
            }

            //foreach (string table in DHTs) {
            //    HuffmanTrees.Add(new HuffmanTree(table));
            //}
        }

        private void GetBinaryData(JPEGFileHandler extractor) {
            string data = extractor.CompressedImage;
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(data[i].ToString(), 16), 2).PadLeft(4, '0'));
            }

            BinaryData = sBuilder.ToString();
        }

        public void Encode() {
            FileHandler.CompressedImage = GetReEncodedData();
        }

        public void Decode() {
            CurrentIndex = 0;
            DecodeBinaryData();
        }

        public void Save(string path) {
            FileHandler.SaveFile(path);
        }


        private void DecodeBinaryData() {
            bool hitEOB = false;
            int count = 0;

            int luminensSubsamling = 1;
            int chrominensSubsampling = 2;

            while (CurrentIndex < BinaryData.Length && BinaryData.Length - CurrentIndex < 8) {

                for (int i = 0; i < luminensSubsamling; i++) {
                    DecodeHuffmanHexValue(HuffmanTable.LumDC, true);
                    for (int j = 0; j < 63 || !hitEOB; j++) {
                        hitEOB = DecodeHuffmanHexValue(HuffmanTable.LumAC, false);
                    }
                }

                for (int i = 0; i < chrominensSubsampling; i++) {
                    DecodeHuffmanHexValue(HuffmanTable.ChromDC, true);
                    for (int j = 0; j < 63 || !hitEOB; j++) {
                        hitEOB = DecodeHuffmanHexValue(HuffmanTable.ChromAC, false);
                    }
                }
                count++;
            }
        }

        private bool DecodeHuffmanHexValue(HuffmanTable table, bool isDC) {
            string huffmanLeafHexValue;
            string amplitude;
            string huffmanTreePath;

            GetHuffmanLeafHexValue(out huffmanTreePath, out huffmanLeafHexValue, table);

            if (huffmanLeafHexValue != "00" || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                amplitude = GetAmplitude(huffmanLeafHexValue);

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

        private string GetHuffmanLeafHexValue(out string currentHuffmanTreePath, out string huffmanLeafHexValue, HuffmanTable table) {
            currentHuffmanTreePath = "";
            huffmanLeafHexValue = "";

            int localCount = 0;

            while (huffmanLeafHexValue == "" && localCount < 16) {
                currentHuffmanTreePath += BinaryData[CurrentIndex];
                huffmanLeafHexValue = HuffmanTrees[(int)table].Find(currentHuffmanTreePath);
                CurrentIndex++;
                localCount++;
            }

            if (huffmanLeafHexValue == "") {
                throw new BinaryPathNotFoundInHuffmanTreeException($"The path {currentHuffmanTreePath} was not found in {table} HuffmanTree.");
            }

            return huffmanLeafHexValue;
        }

        private string GetAmplitude(string huffmanLeafHexValue) {
            string value = "";
            int lenght = Convert.ToInt32(huffmanLeafHexValue[1].ToString(), 16);

            for (int i = 0; i < lenght; i++, CurrentIndex++) {
                value += BinaryData[CurrentIndex];
            }
            return value;
        }

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

            BinaryData = sBuilder.ToString();

            sBuilder.Clear();

            for (int i = 0; i < BinaryData.Length; i += 4) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(BinaryData.Substring(i, 4), 2), 16));
            }

            HexData = sBuilder.ToString();

            return HexData;
        }
    }
}
