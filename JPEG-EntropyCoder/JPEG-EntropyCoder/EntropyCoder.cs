using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Exceptions;
using JPEG_EntropyCoder.Interfaces;
using Utilities;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// Uses HuffmanTrees to code the given JPEG binarydata.
    /// </summary>
    public class EntropyCoder : IEntropyCoder {
        /// <summary>
        /// List with EntropyComponets.
        /// </summary>
        public List<EntropyComponent> EntropyComponents { get; set; }
        private List<IHuffmanTree> HuffmanTrees { get; }
        private BitArray BinaryData { get; }

        private int BitArrayLength { get; }

        /// <summary>
        /// Decodes the given JPEG binarydata.
        /// </summary>
        /// <param name="huffmanTrees">HuffmanTrees from a JPEG image</param>
        /// <param name="binaryData">Binaraydata from a JPEG image. The BitArray needs to be encoded 
        /// with big endian and be in the reverse order.</param>
        /// <param name="luminensBlocks">Luminans blocks per MCU</param>
        public EntropyCoder(List<IHuffmanTree> huffmanTrees, BitArray binaryData, int luminensBlocks) {
            Contract.Requires<ArgumentException>(huffmanTrees.Count == 4);
            Contract.Requires<ArgumentException>(binaryData != null);
            Contract.Requires<ArgumentException>(luminensBlocks > 0);

            HuffmanTrees = huffmanTrees;
            EntropyComponents = new List<EntropyComponent>();
            BinaryData = binaryData;
            BitArrayLength = BinaryData.Count;
            DecodeBinaryData(luminensBlocks);
        }

        /// <summary>
        /// Entropy decodes <see cref="BinaryData"/>.
        /// </summary>
        /// <param name="luminansSubsamling">Amount of luminans blocks per MCU</param>  
        private void DecodeBinaryData(int luminansSubsamling) {
            int chrominansSubsampling = 2;

            while (BinaryData.Count >= 8) {
                DecodeBlock(luminansSubsamling, HuffmanTable.Luminance);
                DecodeBlock(chrominansSubsampling, HuffmanTable.Chrominance);
            }
        }

        /// <summary>
        /// Entropy decodes one block.
        /// </summary>
        /// <param name="subsampling">JPEG image supsampling</param>
        /// <param name="typeTable">Enum that describes the type of block</param>
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
                int count = 0;
                bool hitEOB = false;
                DecodeHuffmanHexValue(DC, true, ref count);
                while (count < 64 && !hitEOB) {
                    hitEOB = DecodeHuffmanHexValue(AC, false, ref count);
                }
            }
        }

        /// <summary>
        /// Entropy decodes one entropycomponent.
        /// </summary>
        /// <param name="table">Enum that describes which huffmanTree to use</param>
        /// <param name="isDC">Bool to know if its an DC EntropyComponent</param>
        /// <returns>Returns true if EOBComponent was created.</returns>
        private bool DecodeHuffmanHexValue(HuffmanTable table, bool isDC, ref int count) {
            byte huffmanLeafByte;
            BitArray amplitude = new BitArray(0);
            BitArray huffmanTreePath;

            GetByteFromHuffmantree(out huffmanTreePath, out huffmanLeafByte, table);
            BinaryData.Length -= huffmanTreePath.Length;
            if (huffmanLeafByte == 0xF0) {
                count += 15;
                EntropyComponents.Add(new ZeroFillComponent(huffmanTreePath, huffmanLeafByte));
            } else if (huffmanLeafByte != 0x00 || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                int length = huffmanLeafByte % 16;
                count += huffmanLeafByte / 16;
                for (int i = BinaryData.Count - 1, j = 0; j < length; i--, j++) {
                    amplitude.Length++;
                    amplitude[j] = BinaryData[i];
                }

                BinaryData.Length -= length;

                if (amplitude.Count == 0) {
                    amplitude.Length += 1;
                    amplitude[0] = false;
                }
                count++;

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

        /// <summary>
        /// This method searches for a leaf in a huffmantree.
        /// </summary>
        /// <param name="currentHuffmanTreePath">The path that found a leaf</param>
        /// <param name="huffmanLeafByte">The byte in the leaf</param>
        /// <param name="table">The HuffmanTree to search in</param>
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
        /// <summary>
        /// Encodes the list EntropyComponets to a BitArray.
        /// </summary>
        /// <returns></returns>
        public BitArray EncodeToBitArray() {
            int currentIndex = 0;

            BitArray bits = new BitArray(BitArrayLength);

            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafByte == 0x00) || entropyComponent is EOBComponent || entropyComponent is ZeroFillComponent) {
                    foreach (bool b in entropyComponent.HuffmanTreePath) {
                        bits[currentIndex++] = b;
                    }
                } else {
                    foreach (bool b in entropyComponent.HuffmanTreePath) {
                        bits[currentIndex++] = b;
                    }
                    foreach (bool b in ((EntropyValueComponent)entropyComponent).Amplitude) {
                        bits[currentIndex++] = b;
                    }
                }
            }

            return bits;
        }

        /// <summary>
        /// Encodes the list EntropyComponets to a byte[].
        /// </summary>
        /// <returns></returns>
        public byte[] EncodeToByteArray() {
            BitArray bits = EncodeToBitArray();

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
    }
}
