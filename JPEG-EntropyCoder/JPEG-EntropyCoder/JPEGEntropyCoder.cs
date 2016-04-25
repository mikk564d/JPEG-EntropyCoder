using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPEG_EntropyCoder.Interfaces;

namespace JPEG_EntropyCoder {
    public class JPEGEntropyCoder : IJPEGEntropyCoder {

        public JPEGEntropyCoder(string path) {
            FileHandler = new JPEGFileHandler(path);
            List<HuffmanTree> huffmanTrees = BuildHuffmanTrees(FileHandler.DHT);
            //foreach (HuffmanTree huffmanTree in huffmanTrees) {
            //    Console.WriteLine("Huffmantree");
            //    foreach (string s in huffmanTree.PrintTree()) {
            //        Console.WriteLine(s);
            //    }
            //}
            Coder = new EntropyCoder(huffmanTrees, ConvertBytesToBitArray(FileHandler.CompressedImage));
        }

        private EntropyCoder Coder { get; }
        private JPEGFileHandler FileHandler { get; }

        private List<HuffmanTree> BuildHuffmanTrees(byte[] DHTFromFile) {
            List<byte[]> DHTs = new List<byte[]>() { new byte[0], new byte[0], new byte[0], new byte[0] };
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
                DHTs[(int)huffmanTree] = dht.ToArray();
            }

            List<HuffmanTree> huffmanTrees = new List<HuffmanTree>();
            foreach (byte[] table in DHTs) {
                huffmanTrees.Add(new HuffmanTree(table));
            }

            return huffmanTrees;
        }

        private BitArray ConvertBytesToBitArray(byte[] bytesFromFile) {
            List<byte> bytes = new List<byte>();

            bytes.Add(bytesFromFile[0]);
            for (int i = 1; i < bytesFromFile.Length; i++) {
                if (!(bytesFromFile[i] == 0x00 && bytesFromFile[i - 1] == 0xFF)) {
                    bytes.Add(bytesFromFile[i]);
                }
            }

            BitArray binData = new BitArray(bytes.ToArray());
            binData = Utility.ReverseBitArray(binData);

            for (int i = 0, j = binData.Count - 1; i < binData.Count / 2; i++, j--) {
                bool temp = binData[i];
                binData[i] = binData[j];
                binData[j] = temp;
            }

            return binData;
        }

        public void Encode() {
            FileHandler.CompressedImage = Coder.EncodeToByteArray();
        }

        public void Save(string path) {
            FileHandler.SaveFile(path);
        }
    }
}
