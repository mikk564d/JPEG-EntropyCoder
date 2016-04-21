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
            List<byte[]> DHTs = new List<byte[]>() { new byte[0] , new byte[0] , new byte[0] , new byte[0] };
            int index = 0;
            for (int i = 0; i < 4; i++) {
                List<byte> dht = new List<byte>();
                int count = 0;
                HuffmanTable huffmanTree;
                if (DHTFromFile[index] < 16) {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTable.LumDC : HuffmanTable.ChromDC;
                } else {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTable.LumAC : HuffmanTable.ChromAC;
                }
                index++;
                for (int j = 0; j < 16; index++, j++) {
                    dht.Add(DHTFromFile[index]);
                    count += Convert.ToInt32(dht[j]);
                }

                for (int k = 0; k < count; index++, k++) {
                    dht.Add(DHTFromFile[index]);
                }
                DHTs[(int) huffmanTree] = dht.ToArray();
            }

            List<HuffmanTree> huffmanTrees = new List<HuffmanTree>();
            foreach (byte[] table in DHTs) {
                huffmanTrees.Add(new HuffmanTree(table));
            }

            return huffmanTrees;
        }

        private BitArray GetBinaryData(JPEGFileHandler extractor) {
            byte[] bytesFromFile = extractor.CompressedImage;
            //Console.WriteLine(BitConverter.ToString(bytesFromFile));
            List<byte> bytes = new List<byte>();

            bytes.Add(bytesFromFile[0]);
            for (int i = 1; i < bytesFromFile.Length; i++) {
                if (!(bytesFromFile[i] == 0x00  && bytesFromFile[i - 1] == 0xFF)) {
                    bytes.Add(bytesFromFile[i]);
                } 
            }

            //Console.WriteLine(BitConverter.ToString(bytes.ToArray()));

            BitArray binData = new BitArray(bytes.ToArray());
            binData = Utility.ReverseBitArray(binData);

            for (int i = 0, j = binData.Count - 1; i < binData.Count / 2; i++, j--) {
                bool temp = binData[i];
                binData[i] = binData[j];
                binData[j] = temp;
            }

            return binData;
        }

        public BitArray GetDataAsBitArray() {
            return new BitArray(GetDataAsByteArray());
        }

        public byte[] GetDataAsByteArray() {
            int currentIndex = 0;

            BitArray bits = new BitArray(0);


            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafByte == 0x00) || entropyComponent is EOBComponent) {
                    foreach (bool b in entropyComponent.HuffmanTreePath) {
                        bits.Length++;
                        bits[currentIndex++] = b;
                    }
                } else {
                    foreach (bool b in entropyComponent.HuffmanTreePath) {
                        bits.Length++;
                        bits[currentIndex++] = b;
                    }
                    foreach (bool b in ((EntropyValueComponent)entropyComponent).Amplitude) {
                        bits.Length++;
                        bits[currentIndex++] = b;
                    }
                }
            }

            while (bits.Count % 8 == 0) {
                bits.Length++;
            }

            byte[] bytesBeforeBitstuff = new byte[bits.Count / 8];

            bits.CopyTo(bytesBeforeBitstuff, 0);

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < bytesBeforeBitstuff.Length; i++) {
                bytes.Add(bytesBeforeBitstuff[i]);
                if (bytesBeforeBitstuff[i] == 0xFF) {
                    bytes.Add(0x00);
                }
            }

            return bytes.ToArray();
        }

        public void Encode() {
            FileHandler.CompressedImage = GetDataAsByteArray();
        }

        public void Decode() {
            DecodeBinaryData();
        }

        public void Save(string path) {
            FileHandler.SaveFile(path);
        }


        private void DecodeBinaryData() {
            int luminensSubsamling = 4;
            int chrominensSubsampling = 2;

            while (BinaryData.Count >= 8) {
                DecodeBlock(luminensSubsamling, "luminens");
                DecodeBlock(chrominensSubsampling, "chrominens");
            }
        }

        private void DecodeBlock(int subsampling, string typeTable) {
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
                bool hitEOB = false;
                DecodeHuffmanHexValue(DC, true);
                for (int j = 0; j < 63 && !hitEOB; j++) {
                    hitEOB = DecodeHuffmanHexValue(AC, false);
                }
            }
        }

        private bool DecodeHuffmanHexValue(HuffmanTable table, bool isDC) {
            byte huffmanLeafByte;
            BitArray amplitude = new BitArray(0);
            BitArray huffmanTreePath;


            GetByteFromHuffmantree(out huffmanTreePath, out huffmanLeafByte, table);
            BinaryData.Length -= huffmanTreePath.Length;

            if (huffmanLeafByte != 0x00 || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                int length = huffmanLeafByte % 16;
                for (int i = BinaryData.Count - 1, j = 0; j < length; i--, j++) {
                    amplitude.Length++;
                    amplitude[j] = BinaryData[i];
                }

                BinaryData.Length -= length;

                if (amplitude.Count == 0) {
                    amplitude.Length += 1;
                    amplitude[0] = false;
                }

                if (isDC) {
                    EntropyComponents.Add(new DCComponent(huffmanTreePath, huffmanLeafByte, amplitude));
                } else {
                    EntropyComponents.Add(new ACComponent(huffmanTreePath, huffmanLeafByte, amplitude));
                }
            } else {
                EntropyComponents.Add(new EOBComponent(huffmanTreePath, huffmanLeafByte));
                return true;
            }

            return false;
        }

        private void GetByteFromHuffmantree(out BitArray currentHuffmanTreePath, out byte huffmanLeafByte, HuffmanTable table) {
            currentHuffmanTreePath = new BitArray(0);
            huffmanLeafByte = 0xFF;

            for (int i = 0, j = BinaryData.Count - 1; i < 16 && huffmanLeafByte == 0xFF; i++, j--) {
                currentHuffmanTreePath.Length++;
                currentHuffmanTreePath[i] = BinaryData[j];
                huffmanLeafByte = HuffmanTrees[(int)table].Find(currentHuffmanTreePath);
            }

            if (huffmanLeafByte == 0xFF) {
                throw new BinaryPathNotFoundInHuffmanTreeException($"The path {currentHuffmanTreePath} was not found in {table} HuffmanTree.");
            }
        }       
    }
}
