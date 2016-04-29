using System.Collections;
using System.Collections.Generic;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Exceptions;
using JPEG_EntropyCoder.Interfaces;
using Utilities;

namespace JPEG_EntropyCoder {
    public class EntropyCoder : IEntropyCoder {
        public EntropyCoder(List<IHuffmanTree> huffmanTrees, BitArray binaryData) {
            HuffmanTrees = huffmanTrees;
            EntropyComponents = new List<EntropyComponent>();
            BinaryData = binaryData;
            DecodeBinaryData();
        }

        public List<EntropyComponent> EntropyComponents { get; set; }
        private List<IHuffmanTree> HuffmanTrees { get; }
        private BitArray BinaryData { get; }


        public BitArray EncodeToBitArray() {
            return BitArrayUtilities.ReverseBitArray(new BitArray(EncodeToByteArray()));
        }

        public byte[] EncodeToByteArray() {
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

            while (bits.Count % 8 != 0) {
                bits.Length++;
                bits[currentIndex++] = true;
            }

            byte[] bytesBeforeBitstuff = new byte[bits.Count / 8];

            bits = BitArrayUtilities.ChangeEndianOnBitArray(bits);

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


        private void DecodeBinaryData() {
            int luminensSubsamling = 1;
            int chrominensSubsampling = 2;

            while (BinaryData.Count >= 8) {
                DecodeBlock(luminensSubsamling, HuffmanTable.Luminance);
                DecodeBlock(chrominensSubsampling, HuffmanTable.Chrominance);
            }
        }

        private void DecodeBlock(int subsampling, HuffmanTable typeTable) {
            HuffmanTable DC;
            HuffmanTable AC;

            if (typeTable == HuffmanTable.Luminance) {
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
