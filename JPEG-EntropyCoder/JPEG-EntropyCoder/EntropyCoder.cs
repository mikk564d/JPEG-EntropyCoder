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
    /// Triggers when progress have been made.
    /// </summary>
    /// <param name="sender">Object which triggered the event</param>
    /// <param name="e">ProgressEventArgs which contains information about the progress</param>
    public delegate void ProgressEvent(object sender, ProgressEventArgs e);

    /// <summary>
    /// Uses HuffmanTrees to code the given JPEG binarydata.
    /// </summary>
    public class EntropyCoder : IEntropyCoder {

        /// <summary>
        /// Event that notifies when progress have been made.
        /// </summary>
        public static event ProgressEvent Progress;

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
            //float progressThreshold = 1;
            int currentProgress = 0;


            while (BinaryData.Count >= 8) {
                DecodeBlock(luminansSubsamling, HuffmanTreeType.Luminance);
                DecodeBlock(chrominansSubsampling, HuffmanTreeType.Chrominance);

                if ((int)((float)(BitArrayLength - BinaryData.Count) / (float)BitArrayLength * 100f) > currentProgress) {
                    currentProgress = (int)((float)(BitArrayLength - BinaryData.Count) / (float)BitArrayLength * 100f);
                    if (Progress != null) {
                        Progress(this, new ProgressEventArgs(currentProgress));
                    }
                }
            }
        }

        /// <summary>
        /// Entropy decodes one block.
        /// </summary>
        /// <param name="subsampling">JPEG image supsampling</param>
        /// <param name="typeTreeType">Enum that describes the type of block</param>
        private void DecodeBlock(int subsampling, HuffmanTreeType typeTreeType) {
            HuffmanTreeType DC;
            HuffmanTreeType AC;

            if (typeTreeType == HuffmanTreeType.Luminance) {
                DC = HuffmanTreeType.LumDC;
                AC = HuffmanTreeType.LumAC;
            } else {
                DC = HuffmanTreeType.ChromDC;
                AC = HuffmanTreeType.ChromAC;
            }         

            for (int i = 0; i < subsampling; i++) {
                int count = 0;
                bool hitEOB = false;
                DecodeComponent(DC, ref count);
                while (count < 64 && !hitEOB) {
                    hitEOB = DecodeComponent(AC, ref count);
                }
            }
        }

        /// <summary>
        /// Entropy decodes one entropycomponent.
        /// </summary>
        /// <param name="treeType">Enum that describes which huffmanTree to use</param>
        /// <param name="count">Counter to know when to stop</param>
        /// <returns>Returns true if EOBComponent was created.</returns>
        private bool DecodeComponent(HuffmanTreeType treeType, ref int count) {
            byte huffmanLeafByte;
            BitVector16 amplitude = new BitVector16();
            BitVector16 huffmanTreePath;

            GetByteFromHuffmantree(out huffmanTreePath, out huffmanLeafByte, treeType);
            BinaryData.Length -= huffmanTreePath.Length;
            if (huffmanLeafByte == 0xF0) {
                count += 15;
                EntropyComponents.Add(new ZeroFillComponent(huffmanTreePath, huffmanLeafByte));
            } else if (huffmanLeafByte != 0x00 || (treeType == HuffmanTreeType.ChromDC || treeType == HuffmanTreeType.LumDC)) {
                int amplitudeLength = huffmanLeafByte % 16;
                /* Increment the counter to know how many values that have been decoded. */
                count += huffmanLeafByte / 16 + 1;
                
                for (int i = BinaryData.Count - 1, j = 0; j < amplitudeLength; i--, j++) {
                    amplitude.Length++;
                    amplitude[(byte) j] = BinaryData[i];
                }
                BinaryData.Length -= amplitudeLength;

                /* In case this is a DC component with huffmanLeafByte 0x00, the amplitude is 0 */
                if (amplitude.Length == 0) {
                    amplitude.Length += 1;
                    amplitude[0] = false;
                }

                if (treeType == HuffmanTreeType.ChromDC || treeType == HuffmanTreeType.LumDC) {
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
        /// <param name="treeType">The HuffmanTree to search in</param>
        private void GetByteFromHuffmantree(out BitVector16 currentHuffmanTreePath, out byte huffmanLeafByte, HuffmanTreeType treeType) {
            currentHuffmanTreePath = new BitVector16();
            huffmanLeafByte = 0xFF;

            for (int i = 0, j = BinaryData.Count - 1; i < 16 && huffmanLeafByte == 0xFF; i++, j--) {
                currentHuffmanTreePath.Length++;
                currentHuffmanTreePath[(byte) i] = BinaryData[j];
                huffmanLeafByte = HuffmanTrees[(int)treeType].Find(currentHuffmanTreePath);
            }

            if (huffmanLeafByte == 0xFF) {
                throw new BinaryPathNotFoundInHuffmanTreeException($"The path {currentHuffmanTreePath} was not found in {treeType} HuffmanTree.");
            }
        }

        /// <summary>
        /// Encodes the list EntropyComponets to a BitArray.
        /// </summary>
        /// <returns></returns>
        public BitArray EncodeToBitArray() {
            int currentIndex = 0;

            BitArray bits = new BitArray(BitArrayLength);

            //int progressThreshold = 1;
            int currentProgress = 0;


            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafByte == 0x00) ||
                           entropyComponent is EOBComponent || entropyComponent is ZeroFillComponent) {
                    for (byte i = 0; i < entropyComponent.HuffmanTreePath.Length; i++) {
                        bits[currentIndex++] = entropyComponent.HuffmanTreePath[i];
                    }
                } else {
                    for (byte i = 0; i < entropyComponent.HuffmanTreePath.Length; i++) {
                        bits[currentIndex++] = entropyComponent.HuffmanTreePath[i];
                    }
                    EntropyValueComponent evc = entropyComponent as EntropyValueComponent;
                    if (evc != null) {
                        for (byte i = 0; i < evc.Amplitude.Length; i++) {
                            bits[currentIndex++] = evc.Amplitude[i];
                        }
                    }
                }


                if ((int)((float)currentIndex / (float)BitArrayLength * 100f) > currentProgress) {
                    currentProgress = (int)((float)currentIndex / (float)BitArrayLength * 100f);
                    Progress?.Invoke(this, new ProgressEventArgs((int)currentProgress));
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

            BitArrayUtilities.ChangeEndianOnBitArray(bits);

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
